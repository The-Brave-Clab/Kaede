using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class InitEndCommand : CommandBase
    {
        public InitEndCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            UIManager.Instance.loadingCanvas.SetActive(false);
            yield return new WaitUntil(() => GameManager.CanPlay);
            GameManager.Play();
#if WEBGL_BUILD
            WebGLInterops.OnScenarioStarted();
#endif
            Debug.Log($"Script {GameManager.ScriptName} Init End.");
            yield return null;
        }
    }
}