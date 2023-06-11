using System;
using UnityEngine;

namespace Y3ADV
{
    [ExecuteAlways]
    public class FillerImageController : MonoBehaviour
    {
        public RectTransform imageLeft;
        public RectTransform imageRight;

        private const float canvasHeight = 1080;

        public void Update()
        {
            float aspectRatio = 16.0f / 9.0f;

            bool playMode = !Application.isEditor || Application.isPlaying;
            bool shouldFix16By9 = playMode && 
                                  GameSettings.Fixed16By9 && Screen.width * 9.0f > Screen.height * 16.0f;
            if (!playMode || !shouldFix16By9)
                aspectRatio = (float) Screen.width / Screen.height;

            float canvasWidth = aspectRatio * canvasHeight;

            imageLeft.anchoredPosition = new Vector2(-canvasWidth / 2, imageLeft.anchoredPosition.y);
            imageRight.anchoredPosition = new Vector2(canvasWidth / 2, imageRight.anchoredPosition.y);
        }
    }
}