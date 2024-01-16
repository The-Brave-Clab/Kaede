using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using NCalc;
using Y3ADV.Database;

namespace Y3ADV
{
    public class Y3ScriptModule : MonoBehaviour, IStateSavable<ScenarioSyncPoint>
    {
        private List<string> statements = null;

        private string currentStatement;
        public string CurrentStatement => string.IsNullOrEmpty(currentStatement) ? "" : currentStatement;
        public int currentStatementIndex = 0;

        private static Dictionary<string, string> aliases = null;
        private static Dictionary<string, Expression> variables = null;

        private static Dictionary<string, Type> commandTypes = null;

        [Header("Global Configs")]
        public bool lipSync = true;
        public float lipSyncScale = 0.25f;

        public bool autoMode { get; set; }
        public bool paused { get; set; }

        private bool shouldSkipMesCommand;
        [NonSerialized]
        public bool mesCommandOnGoing;

        private Coroutine scenarioCoroutine = null;

        public bool ShouldSkipMesCommand
        {
            get
            {
                if (!shouldSkipMesCommand) return false;
                shouldSkipMesCommand = false;
                return true;
            }

            set => shouldSkipMesCommand = mesCommandOnGoing && value;
        }

        public static Y3ScriptModule InstanceInScene => FindObjectOfType<Y3ScriptModule>();

        public int StatementCount()
        {
            return statements.Count;
        }

        public string GetStatementAtIndex(int i)
        {
            if (i < 0 || i >= statements.Count) return null;
            return statements[i];
        }

        private IEnumerator Load(Action<string> onLoaded = null)
        {
            string script = "";
            yield return LocalResourceManager.LoadTextAndPatchFromFile(
                new[] {GameManager.GetScenarioFile(GameManager.ScriptName, "script")},
                false, textAsset => { script = textAsset.text; }, true);

            bool translationLoaded = false;
            
            if (LocalizationSettings.SelectedLocale != LocalizationSettings.ProjectLocale)
            {
                var locale = LocalizationSettings.SelectedLocale;
                yield return LocalResourceManager.LoadTranslation(GameManager.ScriptName,
                    locale.Identifier.CultureInfo.TwoLetterISOLanguageName,
                    textAsset =>
                    {
                        script = LocalizeScript.ApplyTranslation(script, textAsset.text);
                        Debug.Log($"Applied {locale.LocaleName} translation to scenario {GameManager.ScriptName}");
                        translationLoaded = true;
                    });

                if (!translationLoaded)
                {
                    LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.ProjectLocale);
                }
            }

#if WEBGL_BUILD
            WebGLInterops.OnScriptLoaded(script.Replace("\r", ""));
#endif

            onLoaded?.Invoke(script);
        }

        public static List<string> GetStatementsFromScript(string script)
        {
            var lines = script.Split('\n', '\r');
            List<string> result = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("-")) continue; // I really don't think there's a line starts with -
                //trimmed = trimmed.Split(new[] {"//"}, StringSplitOptions.None)[0];
                if (trimmed.StartsWith("//")) continue; // we don't treat // in a valid line as comments any more.
                if (trimmed == "") continue;
                result.Add(trimmed);
            }

            return result;
        }

        private IEnumerator PreloadIncludeFiles(List<string> statements, Dictionary<string, List<string>> includeFiles)
        {
            var includeStatements = statements.Where(s => s.StartsWith("include")).ToList();
            foreach (var s in includeStatements)
            {
                string[] args = s.Split(new[] {'\t'}, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                string includeFileContent = "";
                yield return IncludeCommand.LoadIncludeFile(includeFileName, textAsset =>
                {
                    includeFileContent = textAsset.text;
                });
                Debug.Log($"Pre-Loaded include file {includeFileName}");
                var includeFileStatements = GetStatementsFromScript(includeFileContent);
                includeFiles[includeFileName] = includeFileStatements;
                yield return PreloadIncludeFiles(includeFileStatements, includeFiles);
            }
        }

        private IEnumerator PreprocessInclude(List<string> originalStatements, Dictionary<string, List<string>> includeFiles, List<string> outputStatements)
        {
            foreach (var s in originalStatements)
            {
                if (!s.StartsWith("include"))
                {
                    outputStatements.Add(s);
                    continue;
                }

                string[] args = s.Split(new[] {'\t'}, StringSplitOptions.None);
                string includeFileName = args[1];
                if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
                var includeStatements = includeFiles[includeFileName];
                var processedIncludeStatements = new List<string>();
                yield return PreprocessInclude(includeStatements, includeFiles, processedIncludeStatements);
                outputStatements.AddRange(processedIncludeStatements);
            }
        }

        private IEnumerator PreprocessFunctions(List<string> statements, Action<List<string>> onProcessed)
        {
            Dictionary<string, Y3ScriptFunction> functions = new Dictionary<string, Y3ScriptFunction>();
            Y3ScriptFunction currentFunction = null;
            bool recordingFunction = false;

            List<string> outputStatements = new List<string>();

            foreach (var s in statements)
            {
                // if recording function, just add to current function
                if (recordingFunction && !s.StartsWith("endfunction"))
                {
                    currentFunction.AddStatement(s);
                    continue;
                }

                // if recording and we should end, finish recording
                if (recordingFunction && s.StartsWith("endfunction"))
                {
                    currentFunction.FinishDefinition();
                    functions.Add(currentFunction.functionName, currentFunction);

                    currentFunction = null;
                    recordingFunction = false;
                    continue;
                }

                // if not recording and we should start, start recording
                if (s.StartsWith("function"))
                {
                    currentFunction = new Y3ScriptFunction(this, s);
                    recordingFunction = true;
                    continue;
                }

                // if not recording and we should call a function, call it
                if (s.StartsWith("sub"))
                {
                    var split = s.Split('\t');
                    var functionName = split[1];
                    var parameters = new List<string>(split.Length - 2);
                    for (int i = 2; i < split.Length; ++i)
                    {
                        parameters.Add(split[i]);
                    }
                    
                    if (!functions.ContainsKey(functionName))
                    {
                        Debug.LogError($"Function {functionName} doesn't exist!");
                        continue;
                    }
                    
                    Y3ScriptFunction f = functions[functionName];
                    var functionStatements = f.GetStatements(parameters);

                    outputStatements.AddRange(functionStatements);
                    continue;
                }

                // if not recording and we should do something else, just add it
                outputStatements.Add(s);
            }

            onProcessed(outputStatements);
            yield return null;
        }

        public CommandBase ParseCommand(string statement)
        {
            string[] args = statement.Split(new[] {'\t'}, StringSplitOptions.None);
            string command = args[0];

            Type commandType = commandTypes.ContainsKey(command)
                ? commandTypes[command]
                : typeof(NotImplementedCommand);
            CommandBase commandObj = (CommandBase) System.ComponentModel.TypeDescriptor.CreateInstance(
                provider: null,
                objectType: commandType,
                argTypes: new[] {typeof(Y3ScriptModule), typeof(string)},
                args: new object[] {this, args});

            return commandObj;
        }

        private void Awake()
        {
            aliases = new Dictionary<string, string>();
            variables = new Dictionary<string, Expression>();
            shouldSkipMesCommand = false;
            mesCommandOnGoing = false;

            Application.runInBackground = true;
        }

        private IEnumerator Start()
        {
            string script = "";
            yield return Load(s => script = s);

            RegisterCommands();

            var originalStatements = GetStatementsFromScript(script);
            var includeFiles = new Dictionary<string, List<string>>();
            yield return PreloadIncludeFiles(originalStatements, includeFiles);

            statements = new List<string>();
            yield return PreprocessInclude(originalStatements, includeFiles, statements);

            yield return PreprocessFunctions(statements, s => statements = s);

            StartFromIndex(0);
        }

        private void Stop()
        {
            if (scenarioCoroutine != null)
                StopCoroutine(scenarioCoroutine);
        }

        private void StartFromIndex(int index)
        {
            currentStatementIndex = index;
            scenarioCoroutine = StartCoroutine(ExecuteCommands(statements));
        }

        public IEnumerator ExecuteCommands(List<string> statementsList)
        {
#if !WEBGL_BUILD
            var lastFrameCount = Time.frameCount;
#endif
            while (true)
            {
                var statement = statementsList[currentStatementIndex];
                ++currentStatementIndex;
                currentStatement = statement;
#if !WEBGL_BUILD
                Debug.Log($"[+{Time.frameCount - lastFrameCount}]\t{statement}");
                lastFrameCount = Time.frameCount;
#endif
                CommandBase command;
                try
                {
                    command = ParseCommand(statement);
                }
                catch (Exception ex)
                {
                    Utils.SendBugNotification(ex.ToString());
                    throw;
                }

                if (command.ImmediateExecution)
                {
                    command.MustWait().ForceImmediateExecution();
                    command.Execute().ForceImmediateExecution();
                }
                else
                {
                    yield return command.MustWait().WithException();
                    if (command.SyncExecution)
                    {
                        yield return command.Execute().WithException();
                    }
                    else
                    {
                        StartCoroutine(command.Execute().WithException());
                    }
                }

                if (currentStatementIndex == statementsList.Count)
                    break;
            }

            yield return null;
        }

        private void RegisterCommands()
        {
            commandTypes = new Dictionary<string, Type>
            {
                {"mes", typeof(MesCommand)},
                {"mes_auto", typeof(MesAutoCommand)},
                {"anim", typeof(AnimCommand)},
                {"layer", typeof(LayerCommand)},
                {"move", typeof(MoveCommand)},
                {"pos", typeof(PosCommand)},
                {"rename", typeof(NotImplementedCommand)},
                {"rotate", typeof(RotateCommand)},
                {"scale", typeof(ScaleCommand)},
                {"font", typeof(NotImplementedCommand)},
                {"sprite", typeof(SpriteCommand)},
                {"sprite_hide", typeof(SpriteHideCommand)},
                {"animation_prefab", typeof(AnimationPrefabCommand)},
                {"animation_prefab_hide", typeof(AnimationPrefabHideCommand)},
                {"transform_prefab", typeof(TransformPrefabCommand)},
                {"transform_prefab_hide", typeof(AnimationPrefabHideCommand)},
                {"bg_effect_prefab", typeof(AnimationPrefabCommand)},
                {"bg_effect_prefab_hide", typeof(AnimationPrefabHideCommand)},
                {"del", typeof(DelCommand)},
                {"replace", typeof(ReplaceCommand)},
                {"clone", typeof(NotImplementedCommand)},
                {"color", typeof(ColorCommand)},
                {"alias_text", typeof(AliasTextCommand)},
                {"set", typeof(SetCommand)},
                {"log_message_load", typeof(NotImplementedCommand)},
                {"auto_load", typeof(AutoLoadCommand)},
                {"init_end", typeof(InitEndCommand)},
                {"mes_speed", typeof(NotImplementedCommand)},
                {"move_anim", typeof(MoveAnimCommand)},
                {"rotate_anim", typeof(RotateAnimCommand)},
                {"scale_anim", typeof(NotImplementedCommand)},
                {"move_anim_stop", typeof(MoveAnimStopCommand)},
                {"rotate_anim_stop", typeof(RotateAnimStopCommand)}, // Not tested
                {"scale_anim_stop", typeof(NotImplementedCommand)},
                {"pivot", typeof(PivotCommand)},
                {"include", typeof(IncludeCommand)},
                {"bg", typeof(BGCommand)},
                {"bg_hide", typeof(BGHideCommand)},
                {"bg_move", typeof(NotImplementedCommand)},
                {"actor", typeof(NotImplementedCommand)},
                {"actor_setup", typeof(ActorSetupCommand)},
                {"actor_move", typeof(NotImplementedCommand)},
                {"actor_scale", typeof(ActorScaleCommand)},
                {"actor_show", typeof(ActorShowCommand)},
                {"actor_hide", typeof(ActorHideCommand)},
                {"actor_eye", typeof(ActorEyeCommand)},
                {"actor_face", typeof(ActorFaceCommand)},
                {"actor_enter", typeof(ActorEnterCommand)},
                {"actor_exit", typeof(ActorExitCommand)},
                {"spot_on", typeof(SpotOnCommand)},
                {"spot_off", typeof(SpotOffCommand)},
                {"actor_angle", typeof(ActorAngleCommand)},
                {"actor_body_angle", typeof(ActorBodyAngleCommand)},
                {"actor_auto_mouth", typeof(ActorAutoMouthCommand)},
                {"actor_mouth_sync", typeof(ActorMouthSyncCommand)},
                {"actor_eye_add", typeof(ActorEyeAddCommand)},
                {"actor_eye_abs", typeof(ActorEyeAbsCommand)},
                {"actor_eye_off", typeof(ActorEyeOffCommand)},
                {"actor_auto_del", typeof(ActorAutoDeleteCommand)},
                {"pane_create", typeof(NotImplementedCommand)},
                {"pane_select", typeof(NotImplementedCommand)},
                {"pane_show", typeof(NotImplementedCommand)},
                {"pane_hide", typeof(NotImplementedCommand)},
                {"pane_scale", typeof(NotImplementedCommand)},
                {"pane_del", typeof(NotImplementedCommand)},
                {"pane_rename", typeof(NotImplementedCommand)},
                {"pane_layer", typeof(NotImplementedCommand)},
                {"msg_box_color", typeof(NotImplementedCommand)},
                {"msg_box_show", typeof(MsgBoxShowCommand)},
                {"msg_box_hide", typeof(MsgBoxHideCommand)},
                {"msg_box_change", typeof(NotImplementedCommand)},
                {"msg_box_name_show", typeof(NotImplementedCommand)},
                {"ui_show", typeof(UIShowCommand)},
                {"ui_hide", typeof(UIHideCommand)},
                {"camera_all_on", typeof(CameraAllOnCommand)},
                {"camera_all_off", typeof(CameraAllOffCommand)},
                {"bg_blur_on", typeof(NotImplementedCommand)},
                {"bg_blur_off", typeof(NotImplementedCommand)},
                {"shake", typeof(CameraShakeCommand)},
                {"shake_on", typeof(NotImplementedCommand)},
                {"shake_off", typeof(NotImplementedCommand)},
                {"shake_mes", typeof(MsgBoxShakeCommand)},
                {"focus_on", typeof(NotImplementedCommand)},
                {"focus_off", typeof(NotImplementedCommand)},
                {"fade_in", typeof(FadeInCommand)},
                {"fade_out", typeof(FadeOutCommand)},
                {"camera_lookat", typeof(CameraLookAtCommand)},
                {"camera_move", typeof(CameraMoveCommand)},
                {"camera_zoom", typeof(CameraScaleCommand)}, // Not tested
                {"camera_default", typeof(CameraResetCommand)},
                {"still", typeof(StillCommand)},
                {"still_off", typeof(StillOffCommand)},
                {"still_move", typeof(NotImplementedCommand)},
                {"bgm", typeof(BGMCommand)},
                {"bgm_load", typeof(LoadBGMCommand)},
                {"bgm_stop", typeof(BGMStopCommand)},
                {"se", typeof(SECommand)},
                {"se_load", typeof(LoadSECommand)},
                {"se_stop", typeof(SEStopCommand)},
                {"se_loop", typeof(SELoopCommand)},
                {"voice", typeof(NotImplementedCommand)},
                {"voice_load", typeof(LoadVoiceCommand)},
                {"voice_stop", typeof(NotImplementedCommand)},
                {"voice_play", typeof(NotImplementedCommand)},
                {"asset_load", typeof(NotImplementedCommand)},
                {"asset_unload", typeof(NotImplementedCommand)},
                {"debug_log_show", typeof(NotImplementedCommand)},
                {"caption", typeof(CaptionCommand)},
                {"caption_hide", typeof(CaptionHideCommand)},
                {"caption_color", typeof(CaptionColorCommand)},
                {"caption_font_color", typeof(NotImplementedCommand)},
                {"caption_font_size", typeof(NotImplementedCommand)},
                {"wait", typeof(WaitCommand)},
                // {"function", typeof(FunctionCommand)},
                // {"endfunction", typeof(EndFunctionCommand)},
                // {"sub", typeof(SubCommand)},
                {"end", typeof(EndCommand)},
            };
        }

        public static void AddAlias(string orig, string alias)
        {
            aliases[alias] = orig;
        }

        public static string ResolveAlias(string alias)
        {
            if (aliases == null) return alias;
            string result = alias;
            var sortedKeys = aliases.Keys.ToList();
            sortedKeys.Sort((k2, k1) => k1.Length.CompareTo(k2.Length));
            while (true)
            {
                var replace = sortedKeys.Aggregate(result, 
                    (current, key) => 
                        current.Replace(key, aliases[key]));

                if (replace == result)
                    break;

                result = replace;
            }

            return result;
        }

        public static void AddVariable(string variable, string value)
        {
            if (variable == value)
            {
                Debug.LogError("Variable cannot be equal to value");
                return;
            }

            variables[variable] = new Expression(value);
        }

        public T Evaluate<T>(string expression)
        {
            var exp = new Expression(expression);
            foreach (var v in variables)
            {
                exp.Parameters[v.Key] = v.Value;
            }

            var result = exp.Evaluate();
            return (T) Convert.ChangeType(result, typeof(T));
        }

        public void ExitAdv()
        {
            StartCoroutine(ExitAdvCoroutine());
        }

        public IEnumerator ExitAdvCoroutine()
        {
#if WEBGL_BUILD
            UIManager.Instance.ReplayCanvas.SetActive(true);
            WebGLInterops.OnScenarioFinished();
            yield break;
#else
            // We entered through command line args
            if (StartupSettings.SpecifiedScenario)
            {
                Application.Quit(0);
                yield break;
            }

            GameManager.AddFinishedScenario(GameManager.ScriptName);
            GameManager.SaveFinishedScenario();
            DatabaseManager.Instance.sceneRoot.SetActive(true);

            Application.runInBackground = false;

            yield return SceneManager.UnloadSceneAsync("MainScene");
#endif
        }

        public ScenarioSyncPoint GetState()
        {
            return new()
            {
                currentStatementIndex = currentStatementIndex,

                actors = Y3Live2DManager.AllControllers.Select(c => c.GetState()).ToList(),
                sprites = UIManager.Instance.spriteWrapper.GetComponentsInChildren<SpriteImage>().Select(s => s.GetState()).ToList(),
                backgrounds = UIManager.Instance.backgroundCanvas.GetComponentsInChildren<BackgroundImage>().Select(b => b.GetState()).ToList(),
                stills = UIManager.Instance.stillCanvas.GetComponentsInChildren<BackgroundImage>().Select(b => b.GetState()).ToList(),
                caption = UIManager.Instance.captionBox.GetState(),
            };
        }

        public IEnumerator RestoreState(ScenarioSyncPoint state)
        {
            Stop();

            void CleanAndRestoreStates<T>(Transform parent, List<T> states, Func<T, IEnumerator> restoreState)
                where T : struct
            {
                foreach (var o in parent)
                {
                    Transform t = (Transform) o;
                    Destroy(t.gameObject);
                }

                if (states == null) return;

                foreach (var s in states)
                {
                    GameManager.AddCoroutine(restoreState(s));
                }
            }

            IEnumerator RestoreActorState(ActorState actorState)
            {
                Y3Live2DModelController controller = null;

                var loadedController = Y3Live2DManager.LoadedControllers.FirstOrDefault(c => c.modelName == actorState.name);
                if (loadedController == null)
                {
                    Debug.LogError($"Cannot find actor {actorState.name}!");
                    yield break;
                }
                Y3Live2DManager.CloneLoadedModel(loadedController, clonedController => controller = clonedController);

                yield return controller.RestoreState(actorState);
            }
            IEnumerator RestoreSpriteState(CommonResourceState spriteState)
            {
                GameObject spriteObject = null;
                SpriteImage entity = null;
                yield return SpriteCommand.LoadSprite(spriteState.resourceName,
                    texture2D =>
                    {
                        spriteObject = UIManager.Instance.Sprite(texture2D, spriteState.name, spriteState.resourceName);
                        entity = spriteObject.GetComponent<SpriteImage>();
                    });
                    
                yield return entity.RestoreState(spriteState);
            }
            IEnumerator RestoreBackgroundState(CommonResourceState backgroundState)
            {
                GameObject backgroundObject = null;
                BackgroundImage entity = null;
                yield return BGCommand.LoadBackground(backgroundState.resourceName,
                    texture2D =>
                    {
                        backgroundObject = UIManager.Instance.Background(texture2D, backgroundState.name, backgroundState.resourceName);
                        entity = backgroundObject.GetComponent<BackgroundImage>();
                    });
                    
                yield return entity.RestoreState(backgroundState);
            }
            IEnumerator RestoreStillState(CommonResourceState stillState)
            {
                GameObject stillObject = null;
                BackgroundImage entity = null;
                yield return StillCommand.LoadStill(GameManager.ScriptName, stillState.resourceName,
                    texture2D =>
                    {
                        stillObject = UIManager.Instance.Still(texture2D, stillState.name, stillState.resourceName);
                        entity = stillObject.GetComponent<BackgroundImage>();
                    });
                    
                yield return entity.RestoreState(stillState);
            }

            CleanAndRestoreStates(Y3Live2DManager.Wrapper, state.actors, RestoreActorState);
            CleanAndRestoreStates(UIManager.Instance.spriteWrapper, state.sprites, RestoreSpriteState);
            CleanAndRestoreStates(UIManager.Instance.backgroundCanvas, state.backgrounds, RestoreBackgroundState);
            CleanAndRestoreStates(UIManager.Instance.stillCanvas, state.stills, RestoreStillState);
            GameManager.AddCoroutine(UIManager.Instance.captionBox.RestoreState(state.caption));

            yield return new WaitUntil(GameManager.AllCoroutineFinished);

            StartFromIndex(state.currentStatementIndex);
        }
    }
}