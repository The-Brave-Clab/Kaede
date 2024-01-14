using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ActorEyeAddCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;

        private bool wait = false;

        public ActorEyeAddCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);

            wait = Arg(4, false);
        }

        public override bool SyncExecution => wait;

        public override IEnumerator Execute()
        {
            float addAngle = Arg(2, 0.0f);
            float duration = Arg(3, 0.0f);

            yield return controller.ActorEyeAdd(addAngle, duration);
        }
    }
}