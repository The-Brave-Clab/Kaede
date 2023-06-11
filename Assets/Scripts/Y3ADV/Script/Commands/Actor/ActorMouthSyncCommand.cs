using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorMouthSyncCommand : CommandBase
    {
        private Y3Live2DModelController master = null;
        private Y3Live2DModelController slave = null;
        
        public ActorMouthSyncCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            master = FindEntity<Y3Live2DModelController>(originalArgs[2]);
            slave = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            master.AddMouthSync(slave);

            yield return null;
        }
    }
}
