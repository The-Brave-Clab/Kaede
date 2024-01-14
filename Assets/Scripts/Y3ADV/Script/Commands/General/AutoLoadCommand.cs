using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using live2d;
using UnityEngine;
using UnityEngine.Rendering;

namespace Y3ADV
{
    public class AutoLoadCommand : CommandBase
    {
        public AutoLoadCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        
        private Dictionary<string, string> aliases = null;

        private string ResolveAlias(string alias)
        {
            return aliases.TryGetValue(alias, out var resolvedAlias) ? resolvedAlias : alias;
        }
        
        public override IEnumerator Execute()
        {
            // Ignore file (Not used at the moment)
            // List<string> autoLoadIgnoreList;
            // string ignoreFile = $"{scriptModule.ScriptPath}/{GameManager.ScriptName}_ignore.txt";
            //
            // yield return LocalResourceManager.LoadTextAndPatchFromFile(
            //     new[] {ignoreFile}, 
            //     textAsset =>
            //     {
            //         if (textAsset == null)
            //         {
            //             autoLoadIgnoreList = new List<string>();
            //             return;
            //         }
            //         autoLoadIgnoreList = 
            //             textAsset.text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            //     });

            // First round of scanning the script file
            // Gather aliases and replace subroutines(functions)
            aliases = new Dictionary<string, string>();
            List<string> allStatements = new List<string>(scriptModule.StatementCount());
            Y3ScriptFunction currentRecordingFunction = null;
            Dictionary<string, Y3ScriptFunction> functions = new();

            for (int i = 0; i < scriptModule.StatementCount(); ++i)
            {
                string statement = scriptModule.GetStatementAtIndex(i).Trim(' ', '\t');
                string[] statementArgs = statement.Split(new[] {'\t'}, StringSplitOptions.None);
                bool recordingFunction = currentRecordingFunction != null;
                if (recordingFunction && !statement.StartsWith("endfunction"))
                {
                    currentRecordingFunction.AddStatement(statement);
                    continue;
                }
                if (statementArgs[0] == "sub")
                {
                    List<string> parameters = new List<string>(statementArgs.Length - 2);
                    for (int j = 2; j < statementArgs.Length; ++j)
                    {
                        parameters.Add(statementArgs[j]);
                    }

                    List<string> subStatements = functions[statementArgs[1]].GetStatements(parameters);
                    allStatements.AddRange(subStatements);
                }
                else if (statementArgs[0] == "function")
                {
                    currentRecordingFunction = new Y3ScriptFunction(scriptModule, statement);
                    functions[statementArgs[1]] = currentRecordingFunction;
                }
                else if (statementArgs[0] == "endfunction")
                {
                    currentRecordingFunction = null;
                }
                else
                {
                    allStatements.Add(statement);
                }
            }

            HashSet<LoadData> allLoadData = new();

            // Second round of scanning the script
            // Only process the statements that requires assets
            // Add them to the hashset for coroutine dispatching
            foreach (var statement in allStatements)
            {
                string[] statementArgs = statement.Split(new[] {'\t'}, StringSplitOptions.None);

                switch (statementArgs[0])
                {
                    case "alias_text":
                    {
                        string aliasFile = $"{GameManager.GetScenarioPath()}/{statementArgs[1]}.txt";
                        string aliasFileContent = "";
                        yield return AliasTextCommand.LoadAliasText(aliasFile,
                            textAsset =>
                            {
                                aliasFileContent = textAsset.text;
                            });
                        string[] lines = aliasFileContent.Split('\n', '\r');
                        foreach (var line in lines)
                        {
                            string trimmed = line.Trim();
                            trimmed = trimmed.Split(new[] {"//"}, StringSplitOptions.None)[0];
                            if (trimmed == "") continue;
                            string[] split = trimmed.Split('\t');
                            string alias = split[0];
                            string orig = split[1];
                            aliases.TryAdd(alias, orig);
                        }
                        yield return null;
                        break;
                    }
                    case "人物":
                    case "actor_setup":
                    {
                        string argWithResource = statementArgs[1];
                        string originalResourceName = argWithResource.Split(':')[0];
                        string resourceName = ResolveAlias(originalResourceName);

                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Actor,
                            param1 = originalResourceName,
                            param2 = $"live2d/{resourceName}/model.json",
                            param3 = $"live2d/{resourceName}"
                        });
                        
                        break;
                    }
                    case "font":
                    {
                        break;
                    }
                    case "sprite":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Sprite,
                            param1 = resourceName
                        });
                        break;
                    }
                    case "still":
                    {
                        string argWithResource = statementArgs[1];
                        string still = ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Still,
                            param1 = GameManager.ScriptName,
                            param2 = still
                        });
                        break;
                    }
                    case "背景":
                    case "bg":
                    case "replace":
                    {
                        string bg = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Background,
                            param1 = bg
                        });
                        break;
                    }
                    case "se":
                    case "se_load":
                    case "se_loop":
                    {
                        string assetName = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.SE,
                            param1 = assetName
                        });
                        break;
                    }
                    case "bgm":
                    case "bgm_load":
                    {
                        string assetName = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.BGM,
                            param1 = assetName
                        });
                        break;
                    }
                    case "mes":
                    {
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Voice,
                            param1 = GameManager.ScriptName,
                            param2 = statementArgs[2]
                        });
                        break;
                    }
                    case "voice_load":
                    {
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.Voice,
                            param1 = GameManager.ScriptName,
                            param2 = statementArgs[1]
                        });
                        break;
                    }
                    case "transform_prefab":
                    {
                        string id = statementArgs[2];
                        allLoadData.Add(new()
                        {
                            loadType = LoadData.LoadType.TransformPrefab,
                            param1 = id
                        });
                        break;
                    }
                }
            }
            
            Debug.Log($"Pre-loading {allLoadData.Count} assets...");

            // Sort the hashset by load type
            var loadDataList = allLoadData.ToList();
            loadDataList.Sort((a, b) => a.loadType.CompareTo(b.loadType));

            // Dispatch the coroutines
            foreach (var data in loadDataList)
            {
                switch (data.loadType)
                {
                    case LoadData.LoadType.Sprite:
                    {
                        GameManager.AddCoroutine(SpriteCommand.LoadSprite(data.param1));
                        break;
                    }
                    case LoadData.LoadType.Still:
                    {
                        GameManager.AddCoroutine(StillCommand.LoadStill(data.param1, data.param2));
                        break;
                    }
                    case LoadData.LoadType.Background:
                    {
                        GameManager.AddCoroutine(BGCommand.LoadBackground(data.param1));
                        break;
                    }
                    case LoadData.LoadType.SE:
                    {
                        GameManager.AddCoroutine(LoadSECommand.LoadSE(data.param1));
                        break;
                    }
                    case LoadData.LoadType.BGM:
                    {
                        GameManager.AddCoroutine(LoadBGMCommand.LoadBGM(data.param1));
                        break;
                    }
                    case LoadData.LoadType.Voice:
                    {
                        GameManager.AddCoroutine(LoadVoiceCommand.LoadVoice(data.param1, data.param2));
                        break;
                    }
                    case LoadData.LoadType.TransformPrefab:
                    {
                        GameManager.AddCoroutine(TransformPrefabCommand.LoadTransSprite(int.Parse(data.param1)));
                        break;
                    }
                    case LoadData.LoadType.Actor:
                    {
                        GameManager.AddCoroutine(Y3Live2DManager.LoadModel(new Y3Live2DManager.ModelInfo
                        {
                            Name = data.param1,
                            JsonFile = data.param2,
                            Path = data.param3
                        }));
                        break;
                    }
                }
            }

            yield return new WaitUntil(GameManager.AllCoroutineFinished);
                
            Debug.Log("Resource Pre-loaded");

            yield return null;
        }

        private struct LoadData
        {
            public enum LoadType
            {
                BGM,
                Voice,
                SE,
                Actor,
                Background,
                Still,
                Sprite,
                TransformPrefab,
            }

            public LoadType loadType;
            public string param1;
            public string param2;
            public string param3;

            public override bool Equals(object obj)
            {
                if (obj is LoadData data)
                {
                    return loadType == data.loadType && param1 == data.param1 && param2 == data.param2 &&
                           param3 == data.param3;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (int) loadType * 1234 + 
                       (param1 == null ? 0 : param1.GetHashCode() * 123) + 
                       (param2 == null ? 0 : param2.GetHashCode() * 12) +
                       (param3 == null ? 0 : param3.GetHashCode());
            }
        }
    }
}