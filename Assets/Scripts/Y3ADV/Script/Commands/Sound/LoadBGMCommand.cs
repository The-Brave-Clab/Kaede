using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class LoadBGMCommand : CommandBase
    {
        public LoadBGMCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            yield return LoadBGM(args[1]);
        }

        public static IEnumerator LoadBGM(string bgmName, SoundManager.LoadCallback cb = null)
        {
            string[] allPaths =
            {
                "adv_bgm",
                "jingle"
            };
            List<string> pathsList = new List<string>(allPaths.Length);

            // prioritize path based on filename
            if (bgmName.StartsWith("bgm_adv_"))
                pathsList.Add(allPaths[0]);
            else if (bgmName.StartsWith("jingle_"))
                pathsList.Add(allPaths[1]);

            foreach (var path in allPaths)
                if (!pathsList.Contains(path))
                    pathsList.Add(path);
            
            yield return SoundManager.LoadAudio(
                bgmName,
                pathsList.ToArray(),
                $"{bgmName}.wav",
                SoundManager.SoundType.BGM,
                cb);
        }
    }
}