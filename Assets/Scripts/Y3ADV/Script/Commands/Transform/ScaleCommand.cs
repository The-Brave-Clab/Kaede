using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ScaleCommand : CommandBase
    {
        private BaseEntity controller = null;
        private bool wait = true;
        
        public ScaleCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);

            wait = Arg(5, true);
        }

        public override bool SyncExecution => wait;
        public override IEnumerator Execute()
        {
            float scale = Arg(2, 0.0f);

            Vector3 originalScale = controller.transform.localScale;
            Vector3 targetScale = Vector3.one * (scale * controller.ScaleScalar);
            float duration = Arg(3, 0.0f);
            Ease ease = Arg(4, Ease.Linear);

            yield return controller.Scale(originalScale, targetScale, duration, ease);
        }
    }
}