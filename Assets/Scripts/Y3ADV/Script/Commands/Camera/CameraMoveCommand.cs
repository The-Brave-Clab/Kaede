using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class CameraMoveCommand : CommandBase
    {
        private bool wait = true;

        public CameraMoveCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(4, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            float x = Arg(1, 0.0f);
            float y = Arg(2, 0.0f);
            Vector2 originalPosition = UIManager.CameraPos;
            Vector2 targetPosition = new Vector2(x, y);
            float duration = Arg(3, 0.0f);

            if (duration == 0)
            {
                UIManager.CameraPos = targetPosition;
                yield break;
            }

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => UIManager.CameraPos = value));

            yield return s.WaitForCompletion();
        }
    }
}