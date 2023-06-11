using System.Collections;

namespace Y3ADV
{
    public class EndFunctionCommand : CommandBase
    {
        public EndFunctionCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override bool ShouldForceImmediateExecution => true;
        
        public override IEnumerator Execute()
        {
            scriptModule.StopRecordingFunction();
            yield return null;
        }
    }
}