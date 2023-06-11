using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ColorCommand : CommandBase
    {
        private float r;
        private float g;
        private float b;
        private float a;
        private float duration;
        private Ease ease;
        private bool wait;

        private BaseEntity entity;
        
        public ColorCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            r = Arg(2, 0f);
            g = Arg(3, 0f);            
            b = Arg(4, 0f);         
            a = Arg(5, 1f);
            duration = Arg(6, 1.0f);
            ease = Arg(7, Ease.Linear);
            wait = Arg(8, true);

            entity = FindEntity<BaseEntity>(originalArgs[1]);
        }

        public override bool ShouldWait => wait;
        public override IEnumerator Execute()
        {
            Color originalColor = entity.GetColor();
            Color targetColor = new Color(r, g, b, a);

            yield return entity.ColorCommand(originalColor, targetColor, duration, ease);
        }
    }
}