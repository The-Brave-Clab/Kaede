using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Y3ADV;

namespace Y3ADV
{
    public class IncludeCommand : CommandBase
    {
        public IncludeCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;

        public override IEnumerator Execute()
        {
            Debug.LogError("include command should never be executed directly");

            yield return null;
        }

        public static IEnumerator LoadIncludeFile(string includeFileName,
            LocalResourceManager.ProcessObjectCallback<TextAsset> cb = null)
        {
            if (includeFileName == "define_function") includeFileName = "define_functions"; // a fix
            yield return LocalResourceManager.LoadTextFromFile(
                new[]
                {
                    $"advfunction/{includeFileName}.txt",
                    $"{GameManager.GetScenarioPath()}/{includeFileName}.txt"
                },
                true, false, cb);
        }
    }
}