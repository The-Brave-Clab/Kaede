using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Y3ADV;

namespace Y3ADV
{
    public class NotImplementedCommand : CommandBase
    {
        public NotImplementedCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;

        public override IEnumerator Execute()
        {
            Debug.LogWarning($"Not Implemented Command {args[0]}");
            if (StartupSettings.TestMode)
            {
                TestManager.Fail(TestManager.FailReason.NotImplemented);
                yield return new WaitForSeconds(1.0f);
            }
            yield return null;
        }
    }
}