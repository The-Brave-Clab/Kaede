using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class MsgBoxShowCommand : CommandBase
    {
        public MsgBoxShowCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => false;
        public override IEnumerator Execute()
        {
            UIManager.Instance.messageBox.gameObject.SetActive(true);
            yield return null;
        }
    }
}
