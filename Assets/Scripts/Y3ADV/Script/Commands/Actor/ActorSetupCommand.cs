using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class ActorSetupCommand : CommandBase
    {
        private GameObject obj = null;
        public ActorSetupCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {            
            obj = GameObject.Find(originalArgs[1]);
        }

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            string modelName = Arg(1, "");
            float xPos = Arg(2, 0.0f);
            if (obj != null) yield break;
            Y3Live2DManager.ModelInfo modelInfo = new Y3Live2DManager.ModelInfo()
            {
                Name = originalArgs[1],
                JsonFile = $"live2d/{modelName}/model.json",
                Path = $"live2d/{modelName}"
            };
            yield return Y3Live2DManager.ActorSetup(modelInfo);
        }
    }
}
