using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorEyeAbsCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public ActorEyeAbsCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            float absoluteAngle = Arg(2, 0.0f);

            controller.addEyeX = 0;
            controller.absoluteEyeX = absoluteAngle;

            yield return null;
        }
    }
}