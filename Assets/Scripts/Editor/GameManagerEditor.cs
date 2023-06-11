using UnityEngine;
using UnityEditor;

namespace Y3ADV.Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Script Name", GameManager.ScriptName);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!FixScenario.CanFixScript());
            if (GUILayout.Button("Fix Scenario"))
            {
                FixScenario.Fix();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(!FixScenario.CanReload());
            if (GUILayout.Button("Reload Scenario"))
            {
                FixScenario.Reload();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}