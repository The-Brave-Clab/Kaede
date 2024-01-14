using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ActorBodyAngleCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;

        public ActorBodyAngleCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool SyncExecution => Arg(4, false);

        public override IEnumerator Execute()
        {
            float angleX = Arg(2, 0f);
            float duration = Arg(3, 0f);

            yield return controller.ActorBodyAngle(angleX, duration);
        }
    }
}