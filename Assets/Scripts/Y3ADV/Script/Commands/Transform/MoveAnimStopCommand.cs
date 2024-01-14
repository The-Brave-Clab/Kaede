using System.Collections;
using UnityEngine;

namespace Y3ADV
{
    public class MoveAnimStopCommand : CommandBase
    {
        private BaseEntity controller = null;
        public MoveAnimStopCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            yield return controller.StopAnim("move");
        }
    }
}