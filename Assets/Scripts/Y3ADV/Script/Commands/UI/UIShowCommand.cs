using System.Collections;

namespace Y3ADV
{
    public class UIShowCommand : CommandBase
    {
        public UIShowCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;

        public override IEnumerator Execute()
        {
            UIManager.Instance.uiCanvas.gameObject.SetActive(true);
            yield return null;
        }
    }
}