using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FadeTransition : MonoBehaviour, IStateSavable<FadeState>
    {
        [Range(0, 1)] 
        public float progress = 1;

        public RectTransform mainCanvas;

        // Update is called once per frame
        void Update()
        {
            var pixelRect = mainCanvas.rect;
            
            // if we are in fixed 16:9 mode, adjust pixelRect first
            // only when not in edit mode
            if (!Application.isEditor || Application.isPlaying)
            {
                if (GameSettings.Fixed16By9)
                {
                    if (pixelRect.width * 9 > pixelRect.height * 16)
                    {
                        // Preserve height
                        pixelRect.width = pixelRect.height * 16.0f / 9.0f;
                    }
                }
            }
            
            float length = pixelRect.width + pixelRect.height;
            //float transitionLeft = Mathf.Lerp(0 - transition.sizeDelta.x, mainCanvas.sizeDelta.x, progress);
            float right = Mathf.Lerp(-length, length, progress);
            ((RectTransform) transform).offsetMin = new Vector2(-pixelRect.height, 0);
            ((RectTransform) transform).offsetMax = new Vector2(right, 0);
            //transition.anchoredPosition = new Vector2(transitionLeft, blackPadding.anchoredPosition.y);
            //blackPadding.offsetMax = new Vector2(blackRight, blackPadding.offsetMax.y);
        }

        public FadeState GetState()
        {
            return new()
            {
                progress = progress
            };
        }

        public IEnumerator RestoreState(FadeState state)
        {
            progress = state.progress;
            yield break;
        }
    }
}
