using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class WaitCommand : CommandBase
    {
        public WaitCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            float duration = Arg(1, 0.0f);
            yield return new WaitForSeconds(duration);
        }
    }
}
