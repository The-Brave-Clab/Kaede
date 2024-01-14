using System.Collections;
using UnityEngine;

namespace Y3ADV
{
    public class RotateAnimStopCommand : CommandBase
    {
        private BaseEntity controller = null;
        public RotateAnimStopCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<BaseEntity>(originalArgs[1]);
        }

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            yield return controller.StopAnim("rotate");
        }
    }
}