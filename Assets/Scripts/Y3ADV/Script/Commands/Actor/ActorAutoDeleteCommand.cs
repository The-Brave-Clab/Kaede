using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorAutoDeleteCommand : CommandBase
    {
        
        public ActorAutoDeleteCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            Y3Live2DManager.Instance.autoDeleteActor = Arg(1, false);
            
            yield return null;
        }
    }
}