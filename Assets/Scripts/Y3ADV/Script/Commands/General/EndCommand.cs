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

        public override bool ShouldWait => true;
        public override IEnumerator Execute()
        {
            yield return scriptModule.ExitAdvCoroutine();
        }
    }
}