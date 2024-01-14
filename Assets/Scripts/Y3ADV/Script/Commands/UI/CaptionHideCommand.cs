using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class CaptionHideCommand : CommandBase
    {
        private bool wait = true;
        private float duration = 0.0f;
        public CaptionHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(3, true);
            duration = Arg(2, 0.0f);
        }

        public override bool SyncExecution => wait;
        public override IEnumerator Execute()
        {
            var captionBG = UIManager.Instance.captionBox;

            if (duration <= 0)
            {
                var color = captionBG.color;
                color.a = 0;
                captionBG.color = color;

                color = UIManager.Instance.caption.color;
                color.a = 0;
                UIManager.Instance.caption.color = color;

                UIManager.Instance.captionBox.gameObject.SetActive(false);
                
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(1, 0, duration,
                value =>
                {
                    var color = captionBG.color;
                    color.a = value;
                    captionBG.color = color;

                    color = UIManager.Instance.caption.color;
                    color.a = value;
                    UIManager.Instance.caption.color = color;
                }));
            seq.OnComplete(() => UIManager.Instance.captionBox.gameObject.SetActive(false));

            yield return seq.WaitForCompletion();
        }
    }
}
