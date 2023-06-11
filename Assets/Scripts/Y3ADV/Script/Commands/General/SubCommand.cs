using System.Collections;
using System.Collections.Generic;

namespace Y3ADV
{
    public class SubCommand : CommandBase
    {
        public SubCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            string functionName = Arg(1, "");
            List<string> parameters = new List<string>(args.Length - 2);
            for (int i = 2; i < args.Length; ++i)
            {
                parameters.Add(Arg(i, ""));
            }

            yield return scriptModule.ExecuteFunction(functionName, parameters);
        }
    }
}