using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class RotateCommand : CommandBase
    {
        private BaseEntity controller = null;
        private bool wait = true;
        
        public RotateCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);

            wait = Arg(5, true);
        }

        public override bool ShouldWait => wait;
        public override IEnumerator Execute()
        {
            float targetAngle = Arg(2, 0.0f);

            var eulerAngles = controller.transform.eulerAngles;
            float originalAngle = eulerAngles.z;
            float duration = Arg(3, 0.0f);
            Ease ease = Arg(4, Ease.Linear);

            yield return controller.Rotate(originalAngle, targetAngle, duration, ease);
        }
    }
}