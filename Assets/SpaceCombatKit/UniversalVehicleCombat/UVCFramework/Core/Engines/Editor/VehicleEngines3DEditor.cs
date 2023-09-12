using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Custom editor for the VehicleEngines3D script to enable settings to be specified by force or speed.
    /// </summary>
    [CustomEditor(typeof(VehicleEngines3D))]
    public class VehicleEngines3DEditor : Editor
    {
        protected SerializedProperty controlsDisabledProperty;
        protected SerializedProperty activateEnginesAtStartProperty;

        protected SerializedProperty steeringInputsProperty;
        protected SerializedProperty movementInputsProperty;
        protected SerializedProperty boostInputsProperty;

        protected SerializedProperty minMovementInputsProperty;
        protected SerializedProperty maxMovementInputsProperty;

        protected SerializedProperty onEnginesActivatedProperty;
        protected SerializedProperty onEnginesDeactivatedProperty;

        protected SerializedProperty rigidbodyProperty;

        protected SerializedProperty maxMovementForcesProperty;
        protected SerializedProperty maxSteeringForcesProperty;
        protected SerializedProperty maxBoostForcesProperty;

        protected SerializedProperty movementInputResponseSpeedProperty;

        protected SerializedProperty steeringBySpeedCurveProperty;
        protected SerializedProperty boostSteeringCoefficientProperty;

        protected SerializedProperty boostResourceHandlersListProperty;
        public enum PhysicsSettingsType
        {
            Force,
            Speed
        }

        protected PhysicsSettingsType physicsSettingsType;


        private void OnEnable()
        {
            controlsDisabledProperty = serializedObject.FindProperty("controlsDisabled");
            activateEnginesAtStartProperty = serializedObject.FindProperty("activateEnginesAtStart");

            steeringInputsProperty = serializedObject.FindProperty("steeringInputs");
            movementInputsProperty = serializedObject.FindProperty("movementInputs");
            boostInputsProperty = serializedObject.FindProperty("boostInputs");

            minMovementInputsProperty = serializedObject.FindProperty("minMovementInputs");
            maxMovementInputsProperty = serializedObject.FindProperty("maxMovementInputs");

            onEnginesActivatedProperty = serializedObject.FindProperty("onEnginesActivated");

            onEnginesDeactivatedProperty = serializedObject.FindProperty("onEnginesDeactivated");

            rigidbodyProperty = serializedObject.FindProperty("m_rigidbody");

            maxMovementForcesProperty = serializedObject.FindProperty("maxMovementForces");
            maxSteeringForcesProperty = serializedObject.FindProperty("maxSteeringForces");
            maxBoostForcesProperty = serializedObject.FindProperty("maxBoostForces");

            movementInputResponseSpeedProperty = serializedObject.FindProperty("movementInputResponseSpeed");

            steeringBySpeedCurveProperty = serializedObject.FindProperty("steeringBySpeedCurve");
            boostSteeringCoefficientProperty = serializedObject.FindProperty("boostSteeringCoefficient");

            boostResourceHandlersListProperty = serializedObject.FindProperty("boostResourceHandlers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(controlsDisabledProperty);
            EditorGUILayout.PropertyField(activateEnginesAtStartProperty);

            EditorGUILayout.PropertyField(onEnginesActivatedProperty, true);
            EditorGUILayout.PropertyField(onEnginesDeactivatedProperty, true);

            EditorGUILayout.PropertyField(steeringInputsProperty);
            EditorGUILayout.PropertyField(movementInputsProperty);
            EditorGUILayout.PropertyField(boostInputsProperty);

            EditorGUILayout.PropertyField(minMovementInputsProperty);
            EditorGUILayout.PropertyField(maxMovementInputsProperty);

            EditorGUILayout.PropertyField(rigidbodyProperty);
            Rigidbody rBody = rigidbodyProperty.objectReferenceValue as Rigidbody;
           
            if (rBody == null)
            {
                EditorGUILayout.PropertyField(maxMovementForcesProperty);
                EditorGUILayout.PropertyField(maxSteeringForcesProperty);
                EditorGUILayout.PropertyField(maxBoostForcesProperty);
            }
            else
            {
                // Settings type
                physicsSettingsType = (PhysicsSettingsType)EditorGUILayout.EnumPopup("Physics Settings Type", physicsSettingsType);

                if (physicsSettingsType == PhysicsSettingsType.Force)
                {

                    // Max Movement

                    EditorGUILayout.PropertyField(maxMovementForcesProperty);

                    Vector3 forces = maxMovementForcesProperty.vector3Value;
                    Vector3 speeds = new Vector3(VehicleEngines3D.GetSpeedFromForce(forces.x, rBody),
                                                VehicleEngines3D.GetSpeedFromForce(forces.y, rBody),
                                                VehicleEngines3D.GetSpeedFromForce(forces.z, rBody));

                    GUI.enabled = false;
                    EditorGUILayout.Vector3Field("Max Movement Speeds", speeds);
                    GUI.enabled = true;

                    // Max Steering

                    EditorGUILayout.PropertyField(maxSteeringForcesProperty);

                    forces = maxSteeringForcesProperty.vector3Value;
                    speeds = new Vector3(VehicleEngines3D.GetAngularSpeedFromTorque(forces.x, rBody),
                                            VehicleEngines3D.GetAngularSpeedFromTorque(forces.y, rBody),
                                            VehicleEngines3D.GetAngularSpeedFromTorque(forces.z, rBody));

                    GUI.enabled = false;
                    EditorGUILayout.Vector3Field("Max Steering Rotation Speeds", speeds);
                    GUI.enabled = true;

                    // Max Boost

                    EditorGUILayout.PropertyField(maxBoostForcesProperty);

                    forces = maxBoostForcesProperty.vector3Value;
                    speeds = new Vector3(VehicleEngines3D.GetSpeedFromForce(forces.x, rBody),
                                            VehicleEngines3D.GetSpeedFromForce(forces.y, rBody),
                                            VehicleEngines3D.GetSpeedFromForce(forces.z, rBody));

                    GUI.enabled = false;
                    EditorGUILayout.Vector3Field("Max Boost Speeds", speeds);
                    GUI.enabled = true;

                }
                else
                {
                    // Max Movement

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(maxMovementForcesProperty);
                    GUI.enabled = true;

                    Vector3 forces = maxMovementForcesProperty.vector3Value;
                    Vector3 speeds = new Vector3(VehicleEngines3D.GetSpeedFromForce(forces.x, rBody),
                                                VehicleEngines3D.GetSpeedFromForce(forces.y, rBody),
                                                VehicleEngines3D.GetSpeedFromForce(forces.z, rBody));

                    speeds = EditorGUILayout.Vector3Field("Max Movement Speeds", speeds);

                    forces = new Vector3(VehicleEngines3D.GetForceForSpeed(speeds.x, rBody),
                                            VehicleEngines3D.GetForceForSpeed(speeds.y, rBody),
                                            VehicleEngines3D.GetForceForSpeed(speeds.z, rBody));

                    maxMovementForcesProperty.vector3Value = forces;


                    // Max Steering

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(maxSteeringForcesProperty);
                    GUI.enabled = true;

                    forces = maxSteeringForcesProperty.vector3Value;
                    speeds = new Vector3(VehicleEngines3D.GetAngularSpeedFromTorque(forces.x, rBody),
                                            VehicleEngines3D.GetAngularSpeedFromTorque(forces.y, rBody),
                                            VehicleEngines3D.GetAngularSpeedFromTorque(forces.z, rBody));

                    speeds = EditorGUILayout.Vector3Field("Max Steering Rotation Speeds", speeds);

                    forces = new Vector3(VehicleEngines3D.GetTorqueForAngularSpeed(speeds.x, rBody),
                                            VehicleEngines3D.GetTorqueForAngularSpeed(speeds.y, rBody),
                                            VehicleEngines3D.GetTorqueForAngularSpeed(speeds.z, rBody));

                    maxSteeringForcesProperty.vector3Value = forces;


                    // Max Boost

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(maxBoostForcesProperty);
                    GUI.enabled = true;

                    forces = maxBoostForcesProperty.vector3Value;
                    speeds = new Vector3(VehicleEngines3D.GetSpeedFromForce(forces.x, rBody),
                                            VehicleEngines3D.GetSpeedFromForce(forces.y, rBody),
                                            VehicleEngines3D.GetSpeedFromForce(forces.z, rBody));

                    speeds = EditorGUILayout.Vector3Field("Max Boost Speeds", speeds);

                    forces = new Vector3(VehicleEngines3D.GetForceForSpeed(speeds.x, rBody),
                                            VehicleEngines3D.GetForceForSpeed(speeds.y, rBody),
                                            VehicleEngines3D.GetForceForSpeed(speeds.z, rBody));

                    maxBoostForcesProperty.vector3Value = forces;
                    
                }
            }

            EditorGUILayout.PropertyField(movementInputResponseSpeedProperty);

            EditorGUILayout.PropertyField(steeringBySpeedCurveProperty);
            EditorGUILayout.PropertyField(boostSteeringCoefficientProperty);

            EditorGUILayout.PropertyField(boostResourceHandlersListProperty, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
