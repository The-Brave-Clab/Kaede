using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class MsgBoxHideCommand : CommandBase
    {
        public MsgBoxHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            UIManager.Instance.messageBox.gameObject.SetActive(false);
            UIManager.Instance.nameTag.text = "";
            UIManager.Instance.message.SetText("");
            yield return null;
        }
    }
}