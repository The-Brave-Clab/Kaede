using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorAutoMouthCommand : CommandBase
    {
        public ActorAutoMouthCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            scriptModule.lipSync = Arg(1, true);
            yield return null;
        }
    }
}
