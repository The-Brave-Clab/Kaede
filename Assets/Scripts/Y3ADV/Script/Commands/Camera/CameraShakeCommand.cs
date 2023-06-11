using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class CameraShakeCommand : CommandBase
    {
        private bool wait = true;

        public CameraShakeCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(4, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            float duration = Arg(1, 0f);
            float strength = Arg(2, 20f);
            int vibrato = Arg(3, 10);

            if (duration == 0)
            {
                yield break;
            }

            Vector2 originalPos = UIManager.CameraPos;

            Sequence s = DOTween.Sequence();
            s.Append(DOTween.Punch(
                () => UIManager.CameraPos,
                value =>
                {
                    Vector3 pos = UIManager.CameraPos;
                    pos.x = -value.x; // * canvasScale;
                    UIManager.CameraPos = pos;
                },
                Vector3.one * strength,
                duration,
                vibrato,
                1.0f));

            yield return s.WaitForCompletion();

            UIManager.CameraPos = originalPos;
            yield return null;
        }
    }
}