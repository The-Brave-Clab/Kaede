using System.Collections;

namespace Y3ADV
{
    public class UIHideCommand : CommandBase
    {
        public UIHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => false;

        public override IEnumerator Execute()
        {
            UIManager.Instance.uiCanvas.gameObject.SetActive(false);
            yield return null;
        }
    }
}