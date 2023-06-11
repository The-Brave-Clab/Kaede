using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class BGMStopCommand : CommandBase
    {
        public BGMStopCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            float time = Arg(1, 0.0f);
            SoundManager.Instance.StopBGM(time);
            yield return null;
        }
    }
}
