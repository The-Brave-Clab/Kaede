namespace Y3ADV
{
    public class MesAutoCommand : MesCommand
    {
        public MesAutoCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
    }
}