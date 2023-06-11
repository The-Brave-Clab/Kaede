using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class SpriteHideCommand : CommandBase
    {
        private SpriteImage spriteImage = null;
        private bool wait;
        private float duration;
        
        public SpriteHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);

            spriteImage = FindEntity<SpriteImage>(Arg(1, ""));
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            Image image = spriteImage.GetComponent<Image>();
            
            float targetAlpha = 0.0f;

            yield return spriteImage.ColorAlpha(image.color, image.color.a, targetAlpha, duration, true);
        }
    }
}