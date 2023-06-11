namespace Y3ADV
{
    public class SELoopCommand : SECommand
    {
        public SELoopCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Loop => true;
    }
}