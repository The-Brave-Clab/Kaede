using System.Collections;

namespace Y3ADV
{
    public class CameraAllOffCommand : CommandBase
    {
        public CameraAllOffCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            UIManager.Instance.contentCanvas.gameObject.SetActive(false);
            yield return null;
        }
    }
}