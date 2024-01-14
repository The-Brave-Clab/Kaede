using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ActorAngleCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;

        public ActorAngleCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool SyncExecution => Arg(5, false);

        public override IEnumerator Execute()
        {
            float angleX = Arg(2, 0f);
            float angleY = Arg(3, 0f);
            float duration = Arg(4, 0f);

            yield return controller.ActorAngle(angleX, angleY, duration);
        }
    }
}