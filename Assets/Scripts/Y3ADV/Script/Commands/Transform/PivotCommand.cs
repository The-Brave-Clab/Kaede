using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class PivotCommand : CommandBase
    {
        private RectTransform rectTransform = null;
        private BaseEntity baseEntity = null;
        
        public PivotCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            baseEntity = FindEntity<BaseEntity>(originalArgs[1]);
            rectTransform = baseEntity.GetComponent<RectTransform>();
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            float x = Arg(2, 0.0f);
            float y = Arg(3, 0.0f);

            // Sprite sp = spr.sprite;
            // Texture2D tex = sp.texture;
            // Rect rect = sp.rect;
            // Vector2 pivot = new Vector2(x, y);
            // spr.sprite = Sprite.Create(tex, rect, pivot);
            
            Vector2 anchoredPosition = baseEntity.Position;
            var sizeDelta = rectTransform.sizeDelta;
            var pivot = rectTransform.pivot;
            anchoredPosition.x -= sizeDelta.x * pivot.x;
            anchoredPosition.x += sizeDelta.x * x;
            anchoredPosition.y -= sizeDelta.y * pivot.y;
            anchoredPosition.y += sizeDelta.y * y;
            rectTransform.pivot = new Vector2(x, y);
            baseEntity.Position = anchoredPosition;
            
            yield return null;
        }
    }
}