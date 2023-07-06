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
    public class Y3ScriptModule : MonoBehaviour
    {
        private List<string> statements = null;

        private string currentStatement;
        public string CurrentStatement => string.IsNullOrEmpty(currentStatement) ? "" : currentStatement;

        private static Dictionary<string, string> aliases = null;
        private static Dictionary<string, Expression> variables = null;

        private static Dictionary<string, Type> commandTypes = null;

        private Dictionary<string, Y3ScriptFunction> functions = null;
        private Y3ScriptFunction currentRecordingFunction = null;
        private bool recordingFunction = false;

        private Dictionary<string, List<string>> includeFiles = null;

        [Header("Global Configs")]
        public bool lipSync = true;
        public float lipSyncScale = 0.25f;

        public bool autoMode { get; set; }
        public bool paused { get; set; }

        private bool shouldSkipMesCommand;
        [NonSerialized]
        public bool mesCommandOnGoing;

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

        private IEnumerator Load()
        {
            string script = "";
            yield return LocalResourceManager.LoadTextAndPatchFromFile(
                new[] {GameManager.GetScenarioFile(GameManager.ScriptName, "script")},
                false, textAsset => { script = textAsset.text; });

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

            RegisterCommands();

            var originalStatements = GetStatementsFromScript(script);
            includeFiles = new Dictionary<string, List<string>>();
            yield return PreloadIncludeFiles(originalStatements);

            statements = new List<string>();
            yield return PreprocessInclude(originalStatements, statements);
            StartCoroutine(ExecuteCommands(statements));
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

        private IEnumerator PreloadIncludeFiles(List<string> statements)
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
                yield return PreloadIncludeFiles(includeFileStatements);
            }
        }

        private IEnumerator PreprocessInclude(List<string> originalStatements, List<string> outputStatements)
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
                yield return PreprocessInclude(includeStatements, processedIncludeStatements);
                outputStatements.AddRange(processedIncludeStatements);
            }
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
            functions = new Dictionary<string, Y3ScriptFunction>();
            shouldSkipMesCommand = false;
            mesCommandOnGoing = false;

            Application.runInBackground = true;
        }

        private void Start()
        {
            StartCoroutine(Load());
        }

        public IEnumerator ExecuteCommands(List<string> statementsList)
        {
#if !WEBGL_BUILD
            var lastFrameCount = Time.frameCount;
#endif
            foreach (var statement in statementsList)
            {
                currentStatement = statement;
                if (recordingFunction && !statement.StartsWith("endfunction"))
                {
                    currentRecordingFunction.AddStatement(statement);
                    continue;
                }
#if !WEBGL_BUILD
                if (!recordingFunction)
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

                if (command.ShouldForceImmediateExecution)
                {
                    command.MustWait().ForceImmediateExecution();
                    command.Execute().ForceImmediateExecution();
                }
                else
                {
                    yield return command.MustWait().WithException();
                    if (command.ShouldWait)
                    {
                        yield return command.Execute().WithException();
                    }
                    else
                    {
                        StartCoroutine(command.Execute().WithException());
                    }
                }
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
                {"function", typeof(FunctionCommand)},
                {"endfunction", typeof(EndFunctionCommand)},
                {"sub", typeof(SubCommand)},
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

        public void StartRecordingFunction(string statement)
        {
            currentRecordingFunction = new Y3ScriptFunction(this, statement);
            recordingFunction = true;
        }

        public void StopRecordingFunction()
        {
            currentRecordingFunction.FinishDefinition();
            functions.Add(currentRecordingFunction.functionName, currentRecordingFunction);

            currentRecordingFunction = null;
            recordingFunction = false;
        }

        public IEnumerator ExecuteFunction(string functionName, List<string> args)
        {
            if (!functions.ContainsKey(functionName))
            {
                Debug.LogError($"Function {functionName} doesn't exist!");
                yield break;
            }

            Y3ScriptFunction f = functions[functionName];

            yield return f.Execute(args);
        }

        public List<string> GetFunctionStatements(string functionName)
        {
            if (!functions.ContainsKey(functionName))
            {
                Debug.LogError($"Function {functionName} doesn't exist!");
                return null;
            }

            Y3ScriptFunction f = functions[functionName];
            return f.Statements;
        }

        public bool IsRecordingFunction()
        {
            return recordingFunction;
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
    }
}