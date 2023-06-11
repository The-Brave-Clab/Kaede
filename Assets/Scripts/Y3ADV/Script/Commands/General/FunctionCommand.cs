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

        public override bool ShouldWait => true;
        public override bool ShouldForceImmediateExecution => true;
        
        public override IEnumerator Execute()
        {
            scriptModule.StartRecordingFunction(ToString().Trim());
            
            yield return null;
        }
    }
}