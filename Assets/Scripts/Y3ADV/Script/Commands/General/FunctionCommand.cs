using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class FunctionCommand : CommandBase
    {
        public FunctionCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        public override bool ImmediateExecution => true;
        
        public override IEnumerator Execute()
        {
            scriptModule.StartRecordingFunction(ToString().Trim());
            
            yield return null;
        }
    }
}