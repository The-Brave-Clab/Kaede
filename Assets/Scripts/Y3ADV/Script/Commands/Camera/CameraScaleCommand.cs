using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class CameraScaleCommand : CommandBase
    {
        private bool wait = true;

        public CameraScaleCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(3, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            float scale = Arg(1, 0.0f);
            Vector3 originalScale = UIManager.CameraScale;
            Vector3 targetScale = new Vector3(scale, scale, originalScale.z);
            float duration = Arg(2, 0.0f);

            if (duration == 0)
            {
                UIManager.CameraScale = targetScale;
                yield break;
            }

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalScale, targetScale, duration,
                value => UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}