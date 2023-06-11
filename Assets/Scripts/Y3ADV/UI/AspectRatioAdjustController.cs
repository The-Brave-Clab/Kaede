using System;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    [ExecuteAlways]
    [RequireComponent(typeof(AspectRatioFitter))]
    public class AspectRatioAdjustController : MonoBehaviour
    {
        private AspectRatioFitter fitter;

        private void Awake()
        {
            fitter = GetComponent<AspectRatioFitter>();
        }

        private void Update()
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                fitter.aspectRatio = (float) Screen.width / Screen.height;
            else
#endif
            fitter.aspectRatio = GameSettings.Fixed16By9 && Screen.width * 9.0f > Screen.height * 16.0f
                ? 16.0f / 9.0f
                : (float) Screen.width / Screen.height;
        }
    }
}