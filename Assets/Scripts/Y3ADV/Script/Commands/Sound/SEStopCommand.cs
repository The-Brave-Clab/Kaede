using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class SEStopCommand : CommandBase
    {
        public SEStopCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            string se = Arg<string>(1);
            float duration = Arg(2, 0.0f);
            SoundManager.Instance.StopSE(se, duration);
            yield return null;
        }
    }
}