using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public class SpotOnCommand : CommandBase
    {
        private BaseEntity targetController = null;

        private BaseEntity[] allEntities = null;
        
        public SpotOnCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            targetController = FindEntity<BaseEntity>(originalArgs[1]);

            allEntities = Object.FindObjectsOfType<BaseEntity>().Where(c => c != targetController).ToArray();
        }

        public override bool SyncExecution => false;
        public override bool ImmediateExecution => true;
        public override IEnumerator Execute()
        {
            foreach (var entity in allEntities)
            {
                var alpha = entity.GetColor().a;
                entity.SetColor(new Color(0.3f, 0.3f, 0.3f, alpha));
            }
            var alpha2 = targetController.GetColor().a;
            targetController.SetColor(new Color(1.0f, 1.0f , 1.0f, alpha2));

            yield return null;
        }
    }
}