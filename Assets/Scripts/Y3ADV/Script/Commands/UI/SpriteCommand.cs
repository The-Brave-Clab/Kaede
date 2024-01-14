using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class SpriteCommand : CommandBase
    {
        private string resourceName;
        private string objectName;
        private int layer;
        private float x;
        private float y;
        private float scale;
        private float duration;
        private float alpha;
        private bool wait;

        private GameObject spriteObject = null;
        private Image sprite = null;
        private BaseEntity baseEntity = null;

        public SpriteCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            string name = originalArgs[1];
            var split = name.Split(':');
            resourceName = split[0];
            objectName = split[1];
            layer = Arg(2, 0);
            x = Arg(3, 0f);
            y = Arg(4, 0f);
            scale = Arg(5, 1f);
            duration = Arg(6, 0f);
            alpha = Arg(7, 1f);
            wait = Arg(8, true);
        }

        public override bool SyncExecution => wait;
        public override IEnumerator MustWait()
        {
            yield return LoadSprite(resourceName, 
                texture2D =>
                {
                    spriteObject = UIManager.Instance.Sprite(texture2D, objectName, resourceName);
                    spriteObject.SetActive(false);
                    sprite = spriteObject.GetComponent<Image>();
                    sprite.color = new Color(1, 1, 1, 0);
                    baseEntity = spriteObject.GetComponent<BaseEntity>();
                });
        }

        public override IEnumerator Execute()
        {
            baseEntity.transform.localScale = Vector3.one * (scale * baseEntity.ScaleScalar);
            baseEntity.Position = new Vector3(x, y, layer);
            
            spriteObject.SetActive(true);

            yield return baseEntity.ColorAlpha(sprite.color, 0, alpha, duration, false);
        }

        public static IEnumerator LoadSprite(string spriteName, LocalResourceManager.ProcessObjectCallback<Texture2D> cb = null)
        {
            string[] allPaths =
            {
                $"advmark/{spriteName}.png",
                $"advsprite/{spriteName}.png",
                $"advenemy/{spriteName}.png",
                $"advaccessories/{spriteName}.png",
                $"advcharacter/{spriteName}.png",
            };
            List<string> pathsList = new List<string>(allPaths.Length);

            // prioritize path based on filename
            if (spriteName.StartsWith("adv_mark_"))
                pathsList.Add(allPaths[0]);
            else if (spriteName.StartsWith("scr_"))
                pathsList.Add(allPaths[1]);
            else if (spriteName.StartsWith("enemy_"))
                pathsList.Add(allPaths[2]);
            else if (spriteName.StartsWith("adv_accessory_"))
                pathsList.Add(allPaths[3]);
            else if (spriteName.StartsWith("adv_character_"))
                pathsList.Add(allPaths[4]);

            foreach (var path in allPaths)
                if (!pathsList.Contains(path))
                    pathsList.Add(path);
            
            yield return LocalResourceManager.LoadTexture2DFromFile(pathsList.ToArray(), cb);
        }
    }
}