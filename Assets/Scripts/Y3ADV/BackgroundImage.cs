using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace Y3ADV
{
    [ExecuteInEditMode]
    public class BackgroundImage : BaseEntity, IStateSavable<CommonResourceState>
    {
        public string resourceName;

        public RawImage image = null;
        private RectTransform canvas = null;
        protected override void Awake()
        {
            base.Awake();
            canvas = (RectTransform) UIManager.Instance.contentCanvas.transform;
        }

        // Update is called once per frame
        void Update()
        {
            Resize();
        }

        void Resize()
        {
            var rectTransform = image.rectTransform;
            var pixelRect = canvas.rect;
            
            // if we are in fixed 16:9 mode, adjust pixelRect first
            if (GameSettings.Fixed16By9)
            {
                if (pixelRect.width * 9 > pixelRect.height * 16)
                {
                    // Preserve height
                    pixelRect.width = pixelRect.height * 16.0f / 9.0f;
                }
            }
            
            
            var texture = image.texture;
            if (texture == null)
            {
                rectTransform.sizeDelta = new Vector2(
                    pixelRect.width,
                    pixelRect.height) / ScaleScalar;
                return;
            }
            if (texture.width * pixelRect.height > pixelRect.width * texture.height)
            {
                // Preserve height
                rectTransform.sizeDelta = new Vector2(
                    pixelRect.height * texture.width / texture.height,
                    pixelRect.height) / ScaleScalar;
            }
            else
            {
                // Preserve width
                rectTransform.sizeDelta = new Vector2(
                    pixelRect.width,
                    pixelRect.width * texture.height / texture.width) / ScaleScalar;
            }
        }

        public void SetImage(Texture2D bg)
        {
            image.texture = bg;
            Resize();
        }

        public override Color GetColor()
        {
            return image.color;
        }

        public override void SetColor(Color color)
        {
            image.color = color;
        }

        public CommonResourceState GetState()
        {
            return new()
            {
                name = gameObject.name,
                resourceName = resourceName,
                transform = GetTransformState()
            };
        }

        public IEnumerator RestoreState(CommonResourceState state)
        {
            if (name != state.name || resourceName != state.resourceName)
            {
                Debug.LogError("Applying state to wrong background!");
                yield break;
            }

            RestoreTransformState(state.transform);
        }
    }
}
