using System.Collections;
using System.Collections.Generic;
using biscuit.Scenario.Effect;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class TransformPrefabCommand : CommandBase
    {
        private string resourceName;
        private string objectName;
        private int id;
        private bool wait;

        private CharacterTransformController ctController;

        public TransformPrefabCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            string name = originalArgs[1];
            var split = name.Split(':');
            resourceName = split[0];
            objectName = split[1];
            id = Arg(2, 0);
            wait = Arg(3, true);
        }

        public override bool ShouldWait => wait;
        
        public override IEnumerator MustWait()
        {
            GameObject prefab = Resources.Load<GameObject>(resourceName);
            GameObject instantiated =
                UIManager.Instance.prefabRenderer.Init(prefab, objectName, Vector2.zero, 1);
            
            instantiated.AddComponent<AnimPrefabEntity>();
            ctController = instantiated.GetComponent<CharacterTransformController>();
            yield return LoadTransSprite(id);
        }

        public override IEnumerator Execute()
        {
            ctController.Setup(id);

            yield return null;
        }

        public static IEnumerator LoadTransSprite(int id, LocalResourceManager.ProcessObjectCallback<Texture2D> cb = null)
        {
            string[] allPaths =
            {
                $"adveffect/trans_scene/{id:00}.png",
            };
            
            yield return LocalResourceManager.LoadTexture2DFromFile(allPaths, cb);
        }
    }
}