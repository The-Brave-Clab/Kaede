using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

namespace Y3ADV
{
    public class SECommand : CommandBase
    {
        public SECommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
        protected virtual bool Loop => false;
        public override IEnumerator Execute()
        {
            string se = Arg<string>(1);
            float volume = Arg(2, 1.0f);
            float duration = Arg(3, 0.0f);
            
            SoundManager.Instance.PlaySE(se, volume, duration, Loop);

            yield return null;
        }
    }
}