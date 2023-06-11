using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class CaptionColorCommand : CommandBase
    {
        public CaptionColorCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            // TODO: We ignore the resource name, since there's no two captions appear at the same time FOR NOW.
            //string alias = Arg(1, string.Empty);
            float r = Arg(2, 0f);
            float g = Arg(3, 0f);
            float b = Arg(4, 0f);
            float a = Arg(5, 1f);
            bool setDefault = Arg(6, false);

            Color newColor = new Color(r, g, b, a);

            if (setDefault)
            {
                UIManager.Instance.CaptionDefaultColor = newColor;
            }
            
            UIManager.Instance.captionBox.color = newColor;
            
            yield return null;
        }
    }
}