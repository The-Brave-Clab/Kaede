using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class ReplaceCommand : CommandBase
    {
        private bool wait;
        
        public ReplaceCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(4, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            string resourceName = Arg(1, "");
            string objectName = originalArgs[2];
            float duration = Arg(3, 0.0f);
            
            BackgroundImage originalBGEntity = FindEntity<BackgroundImage>(objectName);
            Transform findBG = originalBGEntity.transform;

            originalBGEntity.gameObject.name = "_REPLACE_" + objectName;

            BackgroundImage newBGEntity = null;

            Color clearWhite = new Color(1, 1, 1, 0);

            yield return BGCommand.LoadBackground(
                resourceName, 
                texture2D =>
                {
                    GameObject clonedObject = Object.Instantiate(findBG.gameObject, findBG.parent);
                    clonedObject.name = objectName;
                    newBGEntity = clonedObject.GetComponent<BackgroundImage>();
                    newBGEntity.SetColor(clearWhite);
                    newBGEntity.image.texture = texture2D;
                    clonedObject.transform.SetSiblingIndex(findBG.GetSiblingIndex() + 1);
                });

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Color(clearWhite, Color.white, duration,
                value => newBGEntity.SetColor(value)));
            seq.OnComplete(() => Object.Destroy(originalBGEntity.gameObject));

            yield return seq.WaitForCompletion();
        }
    }
}