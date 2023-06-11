using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class CaptionCommand : CommandBase
    {
        private bool wait = true;
        private float duration = 0.0f;
        private int fontSize = 0;
        private float x = 0.0f;
        private float y = 0.0f;
        public CaptionCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(6, true);
            duration = Arg(2, 0.0f);
            fontSize = Arg(3, 0);
            x = Arg(4, 0.0f);
            y = Arg(5, 0.0f);
        }

        public override bool ShouldWait => wait;
        public override IEnumerator Execute()
        {
            // TODO: We ignore the resource name, since there's no two captions appear at the same time FOR NOW.
            string caption = args[1].Split(':')[1];
            UIManager.Instance.caption.text = caption;
            UIManager.Instance.caption.fontSize = fontSize;

#if WEBGL_BUILD
            WebGLInterops.OnMessageCommand("caption", "caption", caption);
#endif

            var captionBG = UIManager.Instance.captionBox;
            
            var colorStart = UIManager.Instance.CaptionDefaultColor;
            colorStart.a = 0;
            captionBG.color = colorStart;

            colorStart = UIManager.Instance.caption.color;
            colorStart.a = 0;
            UIManager.Instance.caption.color = colorStart;

            UIManager.Instance.captionBox.gameObject.SetActive(true);

            if (duration <= 0)
            {
                captionBG.color = UIManager.Instance.CaptionDefaultColor;

                var color = UIManager.Instance.caption.color;
                color.a = 1;
                UIManager.Instance.caption.color = color;
                
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(0, 1, duration,
                value =>
                {
                    if (captionBG == null) return;

                    captionBG.color = UIManager.Instance.CaptionDefaultColor;

                    var color = UIManager.Instance.caption.color;
                    color.a = value;
                    UIManager.Instance.caption.color = color;
                }));
            yield return seq.WaitForCompletion();
        }
    }
}
