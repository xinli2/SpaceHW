using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VSX.UniversalVehicleCombat;

[CustomEditor(typeof(RigidbodyCharacterIK))]
public class RigidbodyCharacterIKEditor : Editor
{
    SerializedProperty characterControllerProperty;

    SerializedProperty characterHandleProperty;

    SerializedProperty footRotationLerpSpeedProperty;

    SerializedProperty legsListProperty;

    private void OnEnable()
    {
        characterControllerProperty = serializedObject.FindProperty("characterController");
        characterHandleProperty = serializedObject.FindProperty("characterHandle");
        footRotationLerpSpeedProperty = serializedObject.FindProperty("footRotationLerpSpeed");
        legsListProperty = serializedObject.FindProperty("legs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(characterControllerProperty);
        EditorGUILayout.PropertyField(characterHandleProperty);
        EditorGUILayout.PropertyField(footRotationLerpSpeedProperty);
        
        EditorGUILayout.PropertyField(legsListProperty, true);

        if (GUILayout.Button("Calculate Leg Rotation Offsets"))
        {
            for (int i = 0; i < legsListProperty.arraySize; ++i)
            {
                UpdateRotationOffsets(legsListProperty.GetArrayElementAtIndex(i));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void UpdateRotationOffsets(SerializedProperty legProperty)
    {
        RigidbodyCharacterController characterController = characterControllerProperty.objectReferenceValue as RigidbodyCharacterController;
        if (characterController == null)
        {
            Debug.LogError("Please assign the character controller!");
            return;
        }

        Transform upperLeg = legProperty.FindPropertyRelative("upperLeg").FindPropertyRelative("m_Transform").objectReferenceValue as Transform;
        if (upperLeg == null)
        {
            Debug.LogError("Please assign the upper leg transform!");
            return;
        }

        Transform lowerLeg = legProperty.FindPropertyRelative("lowerLeg").FindPropertyRelative("m_Transform").objectReferenceValue as Transform;
        if (lowerLeg == null)
        {
            Debug.LogError("Please assign the lower leg transform!");
            return;
        }

        Transform foot = legProperty.FindPropertyRelative("foot").FindPropertyRelative("m_Transform").objectReferenceValue as Transform;
        if (foot == null)
        {
            Debug.LogError("Please assign the foot transform!");
            return;
        }

        // Calculate the joint target for the leg
        float jointTargetDistance = ((lowerLeg.position - upperLeg.position).magnitude + (foot.position - lowerLeg.position).magnitude) * 2;

        Vector3 jointTarget = upperLeg.position + characterController.transform.TransformDirection(-Vector3.forward * jointTargetDistance);

        legProperty.FindPropertyRelative("relativeJointTarget").vector3Value = characterController.transform.TransformDirection(-Vector3.forward * jointTargetDistance);

        // Do upper leg
        legProperty.FindPropertyRelative("upperLeg").FindPropertyRelative("rotationOffset").vector3Value = (Quaternion.Inverse(Quaternion.LookRotation(lowerLeg.position - upperLeg.position,
                                            (jointTarget - upperLeg.position).normalized)) * upperLeg.rotation).eulerAngles;


        legProperty.FindPropertyRelative("lowerLeg").FindPropertyRelative("rotationOffset").vector3Value = (Quaternion.Inverse(Quaternion.LookRotation(foot.position - lowerLeg.position,
                                        (jointTarget - upperLeg.position).normalized)) * lowerLeg.rotation).eulerAngles;

        legProperty.FindPropertyRelative("foot").FindPropertyRelative("rotationOffset").vector3Value = (Quaternion.Inverse(Quaternion.LookRotation(characterController.transform.forward, characterController.transform.up) *
                                        foot.rotation)).eulerAngles;
    }
}
