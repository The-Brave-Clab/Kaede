using System.Collections;

namespace Y3ADV
{
    public class ActorScaleCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        private bool wait = true;

        public ActorScaleCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);

            wait = Arg(4, true);
        }

        public override bool SyncExecution => wait;

        public override IEnumerator Execute()
        {
            float scale = Arg(2, 1.0f);
            float duration = Arg(3, 0.0f);

            yield return controller.ActorScale(scale, duration);
        }
    }
}