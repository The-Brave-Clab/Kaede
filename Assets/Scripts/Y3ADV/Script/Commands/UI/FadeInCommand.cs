using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class FadeInCommand : CommandBase
    {
        private bool wait = true;
        private float duration = 1.0f;
        public FadeInCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 1.0f);
            wait = Arg(2, true);
        }

        public override bool SyncExecution => wait;
        public override IEnumerator Execute()
        {
            FadeTransition fade = UIManager.Instance.fade;
            fade.progress = 0;

            if (duration <= 0)
            {
                fade.progress = 1.0f;
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(0, 1, duration,
                value =>
                {
                    fade.progress = value;
                }));
            yield return seq.WaitForCompletion();
        }
    }
}