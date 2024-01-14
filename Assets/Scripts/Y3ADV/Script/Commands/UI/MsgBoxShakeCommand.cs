using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class MsgBoxShakeCommand : CommandBase
    {
        private bool wait = true;

        public MsgBoxShakeCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(4, true);
        }

        public override bool SyncExecution => wait;

        public override IEnumerator Execute()
        {
            float duration = Arg(1, 0f);
            float strength = Arg(2, 20f);
            int vibrato = Arg(3, 10);

            if (duration == 0)
            {
                yield break;
            }

            Vector2 originalPos = UIManager.MessageBoxPos;

            Sequence s = DOTween.Sequence();
            s.Append(DOTween.Punch(
                () => UIManager.MessageBoxPos,
                value =>
                {
                    Vector3 pos = UIManager.MessageBoxPos;
                    pos.x = -value.x;
                    pos.y = -value.y;
                    UIManager.MessageBoxPos = pos;
                },
                Vector3.one * strength,
                duration,
                vibrato,
                1.0f));

            yield return s.WaitForCompletion();

            UIManager.MessageBoxPos = originalPos;
            yield return null;
        }
    }
}