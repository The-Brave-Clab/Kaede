using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class PosCommand : CommandBase
    {
        private BaseEntity controller = null;
        private bool wait = true;
        
        public PosCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(6, true);
            controller = FindEntity<BaseEntity>(originalArgs[1]);
        }

        public override bool ShouldWait => wait;
        public override IEnumerator Execute()
        {
            float x = Arg(2, 0.0f);
            float y = Arg(3, 0.0f);
            Vector3 originalPosition = controller.Position;
            Vector3 targetPosition = new Vector3(x, y, originalPosition.z);
            float duration = Arg(4, 0.0f);
            Ease ease = Arg(5, Ease.Linear);
            
            yield return controller.Move(originalPosition, targetPosition, duration, ease);
        }
    }
}
