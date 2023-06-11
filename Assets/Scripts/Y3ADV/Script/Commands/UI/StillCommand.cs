using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class StillCommand : CommandBase
    {
        public StillCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool ShouldWait => true;

        private GameObject createdObject = null;

        public override IEnumerator MustWait()
        {
            string still = Arg(1, "").Split(':')[0];
            string stillAlias = originalArgs[1].Split(':')[1];

            var existedStill = UIManager.Instance.backgroundCanvas.Find(stillAlias);
            if (existedStill != null)
            {
                createdObject = existedStill.gameObject;
                yield break;
            }

            yield return LoadStill(GameManager.ScriptName, still, 
                texture2D =>
                {
                    createdObject = UIManager.Instance.Still(texture2D, stillAlias);
                    createdObject.SetActive(false);
                });
        }

        public override IEnumerator Execute()
        {
            int layer = Arg(2, 0);
            float x = Arg(3, 0.0f);
            float y = Arg(4, 0.0f);
            float scale = Arg(5, 1.0f);
            
            BackgroundImage controller = createdObject.GetComponent<BackgroundImage>();
            ((RectTransform) controller.transform).localScale = Vector3.one * (scale * controller.ScaleScalar);
            controller.Position = new Vector3(x, y, layer);
            //UIManager.Instance.stillCanvas.GetComponent<Canvas>().sortingOrder = layer;
            
            createdObject.SetActive(true);

            yield return null;
        }

        public static IEnumerator LoadStill(string scriptName, string resourceName, 
            LocalResourceManager.ProcessObjectCallback<Texture2D> cb = null)
        {
            yield return LocalResourceManager.LoadTexture2DFromFile(
                new[]
                {
                    $"adv/{scriptName}/still/{resourceName}.png",
                    $"adv/{scriptName}/{resourceName}.png" // ugly fix for es070_001_m006_a
                }, cb);
        }
    }
}