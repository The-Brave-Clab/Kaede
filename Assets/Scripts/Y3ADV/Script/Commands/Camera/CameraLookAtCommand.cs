using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class CameraLookAtCommand : CommandBase
    {
        private bool wait = true;

        public CameraLookAtCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(5, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            float x = Arg(1, 0.0f);
            float y = Arg(2, 0.0f);
            Vector2 originalPosition = UIManager.CameraPos;
            Vector2 targetPosition = new Vector2(x, y);
            float scale = Arg(3, 1.0f);
            Vector2 originalScale = UIManager.CameraScale;
            Vector2 targetScale = Vector2.one * scale;
            float duration = Arg(4, 0.0f);

            if (duration == 0)
            {
                UIManager.CameraPos = targetPosition;
                UIManager.CameraScale = targetScale;
                yield break;
            }

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => UIManager.CameraPos = value));
            s.Join(DOVirtual.Vector3(originalScale, targetScale, duration,
                value => UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}