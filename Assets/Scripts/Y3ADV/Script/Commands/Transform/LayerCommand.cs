using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class LayerCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public LayerCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            controller.Layer = Arg(2, 0);
            yield return null;
        }
    }
}
