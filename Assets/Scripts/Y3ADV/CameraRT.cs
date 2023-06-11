using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    [RequireComponent(typeof(Camera))]
    public class CameraRT : MonoBehaviour
    {
        public Camera mainCamera = null;
        private RenderTexture rt = null;
        public RawImage targetTexture = null;
        private void OnEnable()
        {
            if (mainCamera == null) return;
            rt = mainCamera.targetTexture;
            RefreshRT();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (rt != null && rt.width == Screen.width && rt.height == Screen.height) return;
            RefreshRT();
        }

        void RefreshRT()
        {
            if (rt != null)
                rt.Release();
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = rt;
            targetTexture.texture = rt;
        }
    }
}
