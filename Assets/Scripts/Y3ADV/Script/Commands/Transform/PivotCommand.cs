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

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            float x = Arg(2, 0.0f);
            float y = Arg(3, 0.0f);
            
            baseEntity.Pivot = new Vector2(x, y);
            yield return null;
        }
    }
}