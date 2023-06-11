using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorEyeOffCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public ActorEyeOffCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            controller.addEyeX = 0;
            controller.absoluteEyeX = 0;

            yield return null;
        }
    }
}