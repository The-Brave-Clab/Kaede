using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Y3ADV;

namespace Y3ADV
{
    public class AliasTextCommand : CommandBase
    {
        public AliasTextCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            string aliasFile = $"{GameManager.GetScenarioPath()}/{args[1]}.txt";
            string aliasFileContent = "";
            yield return LoadAliasText(aliasFile,
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
                Y3ScriptModule.AddAlias(orig, alias);
            }
            yield return null;
        }

        public static IEnumerator LoadAliasText(string aliasFile,
            LocalResourceManager.ProcessObjectCallback<TextAsset> cb = null)
        {
            yield return LocalResourceManager.LoadTextAndPatchFromFile(new[] {aliasFile}, false, cb);
        }
    }
}