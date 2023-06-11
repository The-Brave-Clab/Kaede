using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class LoadSECommand : CommandBase
    {
        public LoadSECommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            yield return LoadSE(args[1]);
        }

        public static IEnumerator LoadSE(string seName, SoundManager.LoadCallback cb = null)
        {
            yield return SoundManager.LoadAudio(
                    seName,
                    new [] {"adv_se"}, 
                    $"{seName}.wav", 
                    SoundManager.SoundType.SE,
                    cb);
        }
    }
}