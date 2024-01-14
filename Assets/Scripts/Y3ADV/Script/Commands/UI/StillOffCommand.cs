using System.Collections;
using UnityEngine;

namespace Y3ADV
{
    public class StillOffCommand : CommandBase
    {
        private GameObject obj = null;
        
        public StillOffCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            obj = FindEntity<BackgroundImage>(originalArgs[1]).gameObject;
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            Object.Destroy(obj);

            yield return null;
        }
    }
}