using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorShowCommand : CommandBase
    {
        private float duration = 0.0f;
        private bool wait = true;
        
        private Y3Live2DModelController controller = null;

        public ActorShowCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);

            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
        }

        public override bool SyncExecution => wait;
        public override IEnumerator Execute()
        {
            yield return new WaitForSeconds(duration);

            controller.hidden = false;
            yield return null;
        }
    }
}