using System.Collections;

namespace Y3ADV
{
    public class EndFunctionCommand : CommandBase
    {
        public EndFunctionCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        public override bool ImmediateExecution => true;
        
        public override IEnumerator Execute()
        {
            scriptModule.StopRecordingFunction();
            yield return null;
        }
    }
}