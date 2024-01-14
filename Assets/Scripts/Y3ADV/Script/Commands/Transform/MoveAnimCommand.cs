using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

namespace Y3ADV
{
    public class MoveAnimCommand : CommandBase
    {
        private BaseEntity controller = null;
        private bool wait = true;
        
        private Vector3 originalPosition = Vector3.zero;
        
        public MoveAnimCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);
            wait = Arg(8, true);
            originalPosition = controller.Position;
        }

        public override bool SyncExecution => wait;
        
        public override IEnumerator Execute()
        {
            float x = Arg(2, 0f);
            float y = Arg(3, 0f);
            float duration = Arg(4, 0f);
            bool rebound = Arg(5, true);
            int loop = Arg(6, 0);
            Ease ease = Arg(7, Ease.Linear);
            Vector3 targetPosition = originalPosition + new Vector3(x, y, 0.0f);

            yield return controller.MoveAnim(originalPosition, targetPosition, duration, rebound, loop,
                ease);
        }
    }
}