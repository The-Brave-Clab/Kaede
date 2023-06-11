using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Y3ADV;

namespace Y3ADV
{
    public class SetCommand : CommandBase
    {
        public SetCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override bool ShouldForceImmediateExecution => true;
        
        public override IEnumerator Execute()
        {
            string[] split = originalArgs[1].Split('=');
            string variable = split[0].Trim();
            string value = split[1].Trim();
            Y3ScriptModule.AddVariable(variable, value);

            yield return null;
        }
    }
}