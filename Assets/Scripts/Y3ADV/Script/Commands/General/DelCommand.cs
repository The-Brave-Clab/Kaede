using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class DelCommand : CommandBase
    {
        private GameObject obj = null;
        
        public DelCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            obj = GameObject.Find(originalArgs[1]);
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            Object.Destroy(obj);

            yield return null;
        }
    }
}
