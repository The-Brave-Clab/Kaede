using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class AnimationPrefabHideCommand : CommandBase
    {
        private GameObject obj = null;
        public AnimationPrefabHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            obj = FindEntity<AnimPrefabEntity>(originalArgs[1]).gameObject;
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            Object.Destroy(obj);
            yield return null;
        }
    }
}