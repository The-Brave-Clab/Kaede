using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Y3ADV.Editor
{
    public static class FixScenario
    {
        static bool isFixingScript;

        [MenuItem("Y3ADV/Scenario/Fix", true)]
        public static bool CanFixScript()
        {
            if (!Application.isPlaying) return false;
            if (string.IsNullOrEmpty(GameManager.ScriptName)) return false;
            if (isFixingScript) return false;
            return true;
        }

        [MenuItem("Y3ADV/Scenario/Fix")]
        public static void Fix()
        {
            string patcherPath = Path.Combine(Application.dataPath, "Scripts", "Editor", "Tools~", "patcher.py");

            string[] args = {patcherPath, GameManager.ScriptName, FileUtil.GetUniqueTempPathInProject()};
            Process process = new Process();
            process.StartInfo.FileName = "python";
            process.StartInfo.Arguments = string.Join(" ", args);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            Thread thread = new Thread(() =>
            {
                isFixingScript = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var path in lines)
                    {
                        if (path.StartsWith("[Not Found]"))
                            Debug.Log(path);
                        else
                        {
                            string content = File.ReadAllText(path);
                            Debug.Log(content);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(errors)) Debug.LogError(errors);
                
                isFixingScript = false;
            });
            
            thread.Start();
        }

        [MenuItem("Y3ADV/Scenario/Reload", true)]
        public static bool CanReload()
        {
            if (!CanFixScript()) return false;
            if (!SceneManager.GetSceneByName("MainScene").isLoaded) return false;
            return true;
        }
        
        [MenuItem("Y3ADV/Scenario/Reload")]
        public static void Reload()
        {
            GameManager.Instance.StartCoroutine(ReloadScenario());
        }

        private static System.Collections.IEnumerator ReloadScenario()
        {
            GameManager.ClearUnityConsole();
            yield return SceneManager.UnloadSceneAsync("MainScene");
            yield return SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
            GameManager.ClearUnityConsole();
        }
    }
}