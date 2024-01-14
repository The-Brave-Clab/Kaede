using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

namespace Y3ADV
{
    public class BGMCommand : CommandBase
    {
        public BGMCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            string bgm = args[1];
            float volume = Arg(2, 1.0f);
            
            SoundManager.Instance.PlayBGM(bgm, volume);

            yield return null;
        }
    }
}
