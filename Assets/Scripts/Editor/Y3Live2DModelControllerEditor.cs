using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Y3ADV.Editor
{
    [CustomEditor(typeof(Y3Live2DModelController))]
    public class Y3Live2DModelControllerEditor : UnityEditor.Editor
    {
        private bool showTestButtons = false;
        private bool showFaceButtons = false;
        private bool showMtnButtons = false;
        private Y3Live2DModelController component = null;
        private void OnEnable()
        {
            //throw new NotImplementedException();
            component = target as Y3Live2DModelController;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Show Debug Window"))
            {
                Live2DDebugWindow.ShowWindow();
            }

            base.OnInspectorGUI();

            if (component != null)
                DrawMotionTestButtons(component, ref showTestButtons, ref showFaceButtons, ref showMtnButtons);
        }

        public static void DrawMotionTestButtons(Y3Live2DModelController controller, ref bool showButtons, ref bool showFace, ref bool showMtn)
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to Test Motions!", MessageType.Warning);
                return;
            }

            showButtons = EditorGUILayout.Foldout(showButtons, "Motion Test Buttons");
            if (showButtons)
            {
                EditorGUI.indentLevel += 1;

                var motionNames = controller.MotionNames;
                motionNames.Sort();

                showFace = EditorGUILayout.Foldout(showFace, "Face Motions");
                if (showFace)
                {
                    EditorGUI.indentLevel += 1;

                    foreach (var motionName in motionNames)
                    {
                        if (!motionName.StartsWith("face_")) continue;
                        if (GUILayout.Button(motionName))
                        {
                            controller.StartMotion(motionName);
                        }
                    }

                    EditorGUI.indentLevel -= 1;
                }

                showMtn = EditorGUILayout.Foldout(showMtn, "Body Motions");
                if (showMtn)
                {
                    EditorGUI.indentLevel += 1;

                    foreach (var motionName in motionNames)
                    {
                        if (!motionName.StartsWith("mtn_")) continue;
                        if (GUILayout.Button(motionName))
                        {
                            controller.StartMotion(motionName);
                        }
                    }
                    
                    EditorGUI.indentLevel -= 1;
                }

                EditorGUI.indentLevel -= 1;
            }
        }
    }

}