using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class AnimationPrefabCommand : CommandBase
    {
        private string resourceName;
        private string objectName;
        private float x;
        private float y;
        private float scale;
        private bool wait;

        public AnimationPrefabCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            string name = originalArgs[1];
            var split = name.Split(':');
            resourceName = split[0];
            objectName = split[1];
            x = Arg(2, 0f);
            y = Arg(3, 0f);
            scale = Arg(4, 1f);
            wait = Arg(5, true);
        }

        public override bool ShouldWait => wait;
        public override bool ShouldForceImmediateExecution => !wait;

        public override IEnumerator Execute()
        {
            GameObject prefab = Resources.Load<GameObject>(resourceName.ToLower());
            GameObject instantiated =
                UIManager.Instance.prefabRenderer.Init(prefab, objectName, Vector2.zero, scale);
            var entity = instantiated.AddComponent<AnimPrefabEntity>();
            entity.Position = new Vector3(x, y, UIManager.Instance.prefabRenderer.prefabPosAnchor.position.z);

            yield return null;
        }
    }
}