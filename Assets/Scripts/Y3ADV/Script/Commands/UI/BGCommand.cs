using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class BGCommand : CommandBase
    {
        public BGCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;

        private GameObject createdObject = null;

        public override IEnumerator MustWait()
        {
            string bg = Arg(1, "");
            string bgAlias = originalArgs[1];

            var existedBg = UIManager.Instance.backgroundCanvas.Find(bgAlias);
            if (existedBg != null)
            {
                createdObject = existedBg.gameObject;
                yield break;
            }

            yield return LoadBackground(bg,
                texture2D =>
                {
                    createdObject = UIManager.Instance.Background(texture2D, bgAlias);
                    createdObject.SetActive(false);
                });
        }

        public override IEnumerator Execute()
        {
            float x = Arg(2, 0.0f);
            float y = Arg(3, 0.0f);
            float scale = Arg(4, 1.0f);
            int layer = Arg(5, 0);
            
            BackgroundImage controller = createdObject.GetComponent<BackgroundImage>();
            ((RectTransform) controller.transform).localScale = Vector3.one * (scale * controller.ScaleScalar);
            controller.Position = new Vector3(x, y, layer);
            //UIManager.Instance.backgroundCanvas.GetComponent<Canvas>().sortingOrder = layer;
            
            createdObject.SetActive(true);

            yield return null;
        }

        public static IEnumerator LoadBackground(string backgroundName,
            LocalResourceManager.ProcessObjectCallback<Texture2D> cb = null)
        {
            yield return LocalResourceManager.LoadTexture2DFromFile(
                new[] {$"advbg/{backgroundName}.png"}, cb);
        }
    }
}