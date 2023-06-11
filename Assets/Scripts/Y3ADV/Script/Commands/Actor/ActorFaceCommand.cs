using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorFaceCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public ActorFaceCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            controller.StartFaceMotion(args[2]);
            yield return null;
        }
    }
}
