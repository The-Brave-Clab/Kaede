using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using live2d;
using live2d.framework;

namespace Y3ADV.Editor
{
    public class Live2DDebugWindow : EditorWindow
    {
        private Y3Live2DModelController controller = null;

        private Vector2 scrollPosition = Vector2.zero;
        
        private bool showMotionTestButtons = false;
        private bool showFaceButtons = false;
        private bool showMotionButtons = false;

        public Live2DDebugWindow()
        {
            Reset();
        }

        private void ResetUI()
        {
            scrollPosition = Vector2.zero;

            showMotionTestButtons = false;
            showFaceButtons = false;
            showMotionButtons = false;

            showRenderTarget = true;

            showLive2DParameters = false;
            showLive2DParts = false;
        }

        private void ResetRef()
        {
            controller = null;
            renderTextureEditor = null;
        }

        private void Reset()
        {
            ResetUI();
            ResetRef();
        }

        [MenuItem("Y3ADV/Debug/Live2D Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<Live2DDebugWindow>("Live2D Debug Window");
        }

        private void OnBecameVisible()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }
        
        private void OnBecameInvisible()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }
        
        private void OnSelectionChanged()
        {
            ResetRef();
            Repaint();
        }
        
        private void OnGUI()
        {
            if (controller == null)
            {
                controller = GetSelectedLive2DModelController();
                if (controller == null)
                {
                    EditorGUILayout.HelpBox("Please select a Live2D model controller in the scene.", MessageType.Info);
                    return;
                }
            }

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            Y3Live2DModelControllerEditor.DrawMotionTestButtons(controller, ref showMotionTestButtons, ref showFaceButtons, ref showMotionButtons);

            var live2DModel = typeof(Y3Live2DModelController)
                .GetField("live2DModel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(controller) as Live2DModelUnity;

            DrawLive2DParameters(live2DModel);
            DrawLive2DParts(live2DModel);

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            var rtPreview = DrawRTPreview();
            
            EditorGUILayout.EndVertical();

            if (Application.isPlaying && rtPreview)
            {
                Repaint();
            }
        }
        
        private Y3Live2DModelController GetSelectedLive2DModelController()
        {
            if (Selection.activeGameObject == null) return null;
            controller = Selection.activeGameObject.GetComponent<Y3Live2DModelController>();
            if (controller == null) return null;
            return controller;
        }

        private bool showRenderTarget = true;
        private UnityEditor.Editor renderTextureEditor = null;
        private bool DrawRTPreview()
        {
            if (controller == null) return false;

            var renderTarget = controller.targetTexture;

            if (renderTarget == null) return false;

            showRenderTarget = EditorGUILayout.Foldout(showRenderTarget, "Render Target");
        
            if (!showRenderTarget) return false;

            // Draw render target
            if (renderTextureEditor == null)
            {
                renderTextureEditor = UnityEditor.Editor.CreateEditor(renderTarget);
            }

            Vector2 windowSize = position.size;
            var previewSize = Mathf.Min(256, windowSize.x);
            var renderTargetRect = GUILayoutUtility.GetRect(previewSize, previewSize);
            renderTextureEditor.OnInteractivePreviewGUI(renderTargetRect, new GUIStyle());

            return true;
        }

        private bool showLive2DParameters = false;
        private void DrawLive2DParameters(Live2DModelUnity model)
        {
            if (model == null) return;

            showLive2DParameters = EditorGUILayout.Foldout(showLive2DParameters, "Live2D Float Parameters");

            if (!showLive2DParameters) return;

            EditorGUI.indentLevel += 1;


            // draw all names of the parameters
            foreach (var paramId in model.GetAllParameters())
            {
                float min = model.GetParameterMin(paramId);
                float max = model.GetParameterMax(paramId);

                float value = model.getParamFloat(paramId);
                string trimmedParamId = paramId;
                if (trimmedParamId.StartsWith("PARAM_"))
                {
                    trimmedParamId = trimmedParamId.Substring(6);
                }
                value = EditorGUILayout.Slider(trimmedParamId, value, min, max);
                model.setParamFloat(paramId, value);
            }


            EditorGUI.indentLevel -= 1;
        }

        private bool showLive2DParts = false;
        private void DrawLive2DParts(Live2DModelUnity model)
        {
            if (model == null) return;

            showLive2DParts = EditorGUILayout.Foldout(showLive2DParts, "Live2D Parts");

            if (!showLive2DParts) return;

            EditorGUI.indentLevel += 1;

            // draw all names of the parameters
            foreach (var partsId in model.GetAllPartsData())
            {
                float value = model.getPartsOpacity(partsId);
                // string trimmedPartsId = partsId;
                // if (trimmedPartsId.StartsWith("PARTS_"))
                // {
                //     trimmedPartsId = trimmedPartsId.Substring(6);
                // }
                value = EditorGUILayout.Slider(partsId, value, 0, 1);
                model.setPartsOpacity(partsId, value);
            }

            EditorGUI.indentLevel -= 1;
        }
    }
}