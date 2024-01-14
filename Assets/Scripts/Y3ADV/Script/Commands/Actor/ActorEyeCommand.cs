using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorEyeCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public ActorEyeCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            string state = args[2];
            controller.SetEye(state);
            yield return null;
        }
    }
}