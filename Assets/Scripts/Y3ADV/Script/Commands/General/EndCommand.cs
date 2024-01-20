using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y3ADV.Database;

namespace Y3ADV
{
    public class EndCommand : CommandBase
    {
        public EndCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            scriptModule.initialized = false;
            yield return scriptModule.ExitAdvCoroutine();
        }
    }
}