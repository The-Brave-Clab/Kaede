using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public class SpotOffCommand : CommandBase
    {
        private BaseEntity[] allEntities = null;
        
        public SpotOffCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            allEntities = Object.FindObjectsOfType<BaseEntity>().ToArray();
        }

        public override bool SyncExecution => false;
        public override bool ImmediateExecution => true;
        public override IEnumerator Execute()
        {
            foreach (var entity in allEntities)
            {
                var alpha = entity.GetColor().a;
                entity.SetColor(new Color(1.0f, 1.0f, 1.0f, alpha));
            }

            yield return null;
        }
    }
}