using System.Collections;

namespace Y3ADV
{
    public class IntentionallyNotImplementedCommand : CommandBase
    {
        public IntentionallyNotImplementedCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;

        public override IEnumerator Execute()
        {
            yield return null;
        }
    }
}