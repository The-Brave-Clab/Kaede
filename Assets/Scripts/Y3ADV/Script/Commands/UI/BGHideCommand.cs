using System.Collections;
using UnityEngine;

namespace Y3ADV
{
    public class BGHideCommand : CommandBase
    {
        private GameObject obj = null;
        public BGHideCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            obj = FindEntity<BackgroundImage>(originalArgs[1]).gameObject;
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            obj.SetActive(false);
            yield return null;
        }
    }
}