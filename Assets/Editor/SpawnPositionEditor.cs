#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using DDS;

[CustomEditor(typeof(SpawnPosition))]
public class SpawnPositionEditor : Editor
{
    SerializedProperty GroundDetectionHeight;
    SerializedProperty ObjectsToSpawn;
    SerializedProperty ObjectsToIgnore;

    SpawnPosition SerializedObject;

    GUILayoutOption[] ButtonLayout;

    void Awake()
    {
        SerializedObject = target as SpawnPosition;

        GroundDetectionHeight = this.serializedObject.FindProperty("GroundDetectionHeight");
        ObjectsToSpawn = this.serializedObject.FindProperty("Objects_to_Spawn");
        ObjectsToIgnore = this.serializedObject.FindProperty("IgnoredSpawnObject");

        ButtonLayout = new GUILayoutOption[2];
        ButtonLayout[0] = GUILayout.Height(15);
        ButtonLayout[1] = GUILayout.Width(100);
    }

    override public void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();


        //EditorGUILayout.PropertyField();
        EditorGUILayout.PropertyField(ObjectsToIgnore);
        EditorGUILayout.PropertyField(GroundDetectionHeight);

        int ObjectsToSpawnSize = ObjectsToSpawn.arraySize;

        for (int i = 0; i < ObjectsToSpawn.arraySize; i++)
        {
            string ObjectName = "Empty";

            if (ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue != null)
                ObjectName = ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue.name;

            EditorGUILayout.PropertyField(ObjectsToSpawn.GetArrayElementAtIndex(i), new GUIContent(ObjectName), true);
        }

        if (ObjectsToSpawn.arraySize < 10)
        {
            if (GUILayout.Button(new GUIContent("Add Element"), ButtonLayout))
            {
                Array.Resize(ref SerializedObject.Objects_to_Spawn, SerializedObject.Objects_to_Spawn.Length + 1);
                SerializedObject.Objects_to_Spawn[SerializedObject.Objects_to_Spawn.Length - 1] = new SpawnAbleObject();
            }
        }


        EditorGUI.EndChangeCheck();

        this.serializedObject.ApplyModifiedProperties();


    }
}
#endif