using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class RotateAnimCommand : CommandBase
    {
        private BaseEntity controller = null;
        private bool wait = true;

        private float originalAngle = 0;
        
        public RotateAnimCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);
            wait = Arg(7, true);
            
            Vector3 eulerAngles = controller.transform.eulerAngles;
            originalAngle = eulerAngles.z;
        }

        public override bool SyncExecution => wait;
        
        public override IEnumerator Execute()
        {
            float angle = Arg(2, 0f);
            float duration = Arg(3, 0f);
            bool rebound = Arg(4, true);
            int loop = Arg(5, 0);
            Ease ease = Arg(6, Ease.Linear);
            float targetAngle = originalAngle + angle;

            yield return controller.RotateAnim(originalAngle, targetAngle, duration, rebound, loop, ease);
        }
    }
}