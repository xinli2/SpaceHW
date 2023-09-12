using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat.Loadout
{
    [CustomEditor(typeof(LoadoutDataManagerJSON))]
    public class UVCLoadoutDataManagerEditor : Editor
    {
        SerializedProperty fileNameProperty;
        SerializedProperty debugProperty;

        protected virtual void OnEnable()
        {
            fileNameProperty = serializedObject.FindProperty("fileName");
            debugProperty = serializedObject.FindProperty("debug");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(fileNameProperty);

            EditorGUILayout.PropertyField(debugProperty);

            if (GUILayout.Button("Delete Saved Data"))
            {
                LoadoutDataManagerJSON script = (LoadoutDataManagerJSON)target;
                script.DeleteSaveData();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
