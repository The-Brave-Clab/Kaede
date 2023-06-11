using System.Collections;

namespace Y3ADV
{
    public class CameraAllOnCommand : CommandBase
    {
        public CameraAllOnCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            UIManager.Instance.contentCanvas.gameObject.SetActive(true);
            yield return null;
        }
    }
}