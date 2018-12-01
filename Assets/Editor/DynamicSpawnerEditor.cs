#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DDS;


[CustomEditor(typeof(Spawner))]
public class DynamicSpawnerEditor : Editor
{
    SerializedProperty CurrentContiniousWaveStatus;
    SerializedProperty foldOutObjectsToSpawn;
    SerializedProperty ShowPointPositions;
    SerializedProperty IgnoredObjects;
    SerializedProperty SpawnDelay;
    SerializedProperty ShowIgnoredObjects;
    SerializedProperty WaveSpawnAmount;
    SerializedProperty MaximalSpawnedObjectsAlive;
    SerializedProperty RangeToCheck;
    SerializedProperty DoSpawnIfNotInRange;
    SerializedProperty DoSpawnContiniousWaves;
    SerializedProperty DoSpawnInFrustum;
    SerializedProperty DoLimitObjectsAlive;
    SerializedProperty IsNotInRange;
    SerializedProperty SpawnPositions;
    SerializedProperty Player;
    SerializedProperty ObjectToSpawn;
    SerializedProperty SelectedSpawnPositionOption;
    SerializedProperty SelectedSpawningStyle;
    SerializedProperty SelectedPlayerIdentification;
    SerializedProperty SelectedDistanceCheck;
    SerializedProperty PlayerIdentificationData;
    SerializedProperty FrustumCamera;
    SerializedProperty UseOcclusionCulling;
    SerializedProperty ObjectsToSpawn;
    SerializedProperty TriggerSpawnOverridesLogic;
    SerializedProperty ActiveSpawnPoint;

    GUILayoutOption StandardLayout = GUILayout.Height(15);

    Spawner DynamicSpawned;

    protected virtual void OnEnable()
    {
        DynamicSpawned = target as Spawner;

        ActiveSpawnPoint = this.serializedObject.FindProperty("SelectedSpawnPosition");
        CurrentContiniousWaveStatus = this.serializedObject.FindProperty("CurrentContiniousWaveStatus");
        foldOutObjectsToSpawn = this.serializedObject.FindProperty("FoldoutObjectsToSpawn");
        TriggerSpawnOverridesLogic = this.serializedObject.FindProperty("TriggerSpawnOverridesLogic");
        ObjectsToSpawn = this.serializedObject.FindProperty("ObjectsToSpawn");
        UseOcclusionCulling = this.serializedObject.FindProperty("UseOcclusionCulling");
        ShowPointPositions = this.serializedObject.FindProperty("DoShowPointPositions");
        IgnoredObjects = this.serializedObject.FindProperty("IgnoredObjects");
        SpawnDelay = this.serializedObject.FindProperty("SpawnDelay");
        ShowIgnoredObjects = this.serializedObject.FindProperty("ShowIgnoredObjects");
        WaveSpawnAmount = this.serializedObject.FindProperty("WaveSpawnAmount");
        MaximalSpawnedObjectsAlive = this.serializedObject.FindProperty("MaximalSpawnedObjectsAlive");
        RangeToCheck = this.serializedObject.FindProperty("RangeToCheck");
        DoSpawnIfNotInRange = this.serializedObject.FindProperty("DoSpawnIfNotInRange");
        DoSpawnContiniousWaves = this.serializedObject.FindProperty("DoSpawnContinuousWaves");
        DoSpawnInFrustum = this.serializedObject.FindProperty("DoSpawnInFrustum");
        DoLimitObjectsAlive = this.serializedObject.FindProperty("DoLimitObjectsAlive");
        IsNotInRange = this.serializedObject.FindProperty("IsNotInRange");
        SpawnPositions = this.serializedObject.FindProperty("SpawnPositions");
        Player = this.serializedObject.FindProperty("Player");
        ObjectToSpawn = this.serializedObject.FindProperty("ObjectToSpawn");
        SelectedSpawnPositionOption = this.serializedObject.FindProperty("SelectedSpawnPositionOption");
        SelectedSpawningStyle = this.serializedObject.FindProperty("SelectedSpawningStyle");
        SelectedPlayerIdentification = this.serializedObject.FindProperty("SelectedPlayerIdentification");
        SelectedDistanceCheck = this.serializedObject.FindProperty("SelectedDistanceCheck");
        PlayerIdentificationData = this.serializedObject.FindProperty("PlayerIdentificationData");
        FrustumCamera = this.serializedObject.FindProperty("FrustumCamera");
    }

    [MenuItem("GameObject/Dynamic Spawning System/SpawnPoint", false, 0)]
    static void AddSpawnPoint()
    {
        GameObject NewSpawnPoint = Instantiate(Resources.Load("SpawnPosition") as GameObject, Selection.activeTransform);

        NewSpawnPoint.name = "New Spawn Point";
    }

    [MenuItem("GameObject/Dynamic Spawning System/SpawnNode", false, 0)]
    static void CreateSpawnNode()
    {
        GameObject NewSpawnNode = Instantiate(Resources.Load("Spawn Node") as GameObject);
        NewSpawnNode.name = "New Spawn Node";
    }

    bool FoldOut;

    override public void OnInspectorGUI()
    {

        this.serializedObject.Update();

        SpawningFunctions.TriggerSpawnOverridesLogic = TriggerSpawnOverridesLogic.boolValue;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(SelectedSpawningStyle, new GUIContent("Spawn Style: "), StandardLayout);

        EditorGUI.indentLevel++;

        if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Continuous)
        {
            EditorGUILayout.PropertyField(SpawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);

            if (SpawnDelay.floatValue < 0f)
                SpawnDelay.floatValue *= -1;

            EditorGUILayout.PropertyField(DoLimitObjectsAlive, new GUIContent("Limit Object Amount: "), StandardLayout);

            if (DoLimitObjectsAlive.boolValue)
            {
                EditorGUILayout.PropertyField(MaximalSpawnedObjectsAlive, new GUIContent("Amount: "), StandardLayout);

                if (MaximalSpawnedObjectsAlive.intValue < 0)
                    MaximalSpawnedObjectsAlive.intValue = 0;
            }
        }

        if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
        {
            EditorGUILayout.PropertyField(DoSpawnContiniousWaves, new GUIContent("Continious Waves: "), StandardLayout);

            if (DoSpawnContiniousWaves.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(CurrentContiniousWaveStatus, new GUIContent("Current Status: "), true);
                EditorGUI.indentLevel--;
            }

            else
            {
                EditorGUILayout.PropertyField(SpawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);
            }

            EditorGUILayout.PropertyField(WaveSpawnAmount, new GUIContent("Amount: "), StandardLayout);

            if (WaveSpawnAmount.intValue > 100)
                WaveSpawnAmount.intValue = 100;

            if (WaveSpawnAmount.intValue < 1)
                WaveSpawnAmount.intValue = 1;


            SpawningFunctions.WaveSpawnAmount = WaveSpawnAmount.intValue;
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(DoSpawnIfNotInRange, new GUIContent("Spawn if not in Range: "), StandardLayout);

        if (DoSpawnIfNotInRange.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(SelectedPlayerIdentification, new GUIContent("Identification: "), StandardLayout);

            switch ((IdentifyPlayer)SelectedPlayerIdentification.intValue)
            {
                case IdentifyPlayer.byField:
                    EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Object"), new GUIContent("Object: "), StandardLayout);
                    break;

                case IdentifyPlayer.byName:
                    EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Name"), new GUIContent("Name: "), StandardLayout);
                    break;

                case IdentifyPlayer.byTag:
                    string[] Tags = UnityEditorInternal.InternalEditorUtility.tags;
                    PlayerIdentificationData.FindPropertyRelative("Tag").intValue = EditorGUILayout.Popup(new GUIContent("Tag: ", "How to check for the Player"), PlayerIdentificationData.FindPropertyRelative("Tag").intValue, Tags);
                    break;
            }

            EditorGUILayout.PropertyField(SelectedDistanceCheck, new GUIContent("Check Style: "), StandardLayout);

            if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.ThreeDimensionalCheck || SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.TwoDimensionalCheck)
            {
                EditorGUILayout.PropertyField(RangeToCheck, new GUIContent("Range: "), StandardLayout);

                if (DynamicSpawned.gameObject.GetComponent<SphereCollider>())
                    DestroyImmediate(DynamicSpawned.gameObject.GetComponent<SphereCollider>());
            }

            else if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.SphereColliderCheck)
            {
                if (DynamicSpawned.GetComponent<SphereCollider>() == null)
                {
                    DynamicSpawned.gameObject.AddComponent<SphereCollider>();
                }

                DynamicSpawned.gameObject.GetComponent<SphereCollider>().isTrigger = true;

                DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius = EditorGUILayout.FloatField(new GUIContent("Sphere Radius: "), DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius);

                DynamicSpawned.gameObject.GetComponent<SphereCollider>().hideFlags = HideFlags.HideInInspector;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(DoSpawnInFrustum, new GUIContent("Spawn in Frustum: "), StandardLayout);

        if (!DoSpawnInFrustum.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(FrustumCamera, new GUIContent("Camera: "), StandardLayout);

            EditorGUILayout.PropertyField(UseOcclusionCulling, new GUIContent("Occlusion Culling: "), StandardLayout);

            if (UseOcclusionCulling.boolValue)
            {
                EditorGUILayout.PropertyField(IgnoredObjects, new GUIContent("Ignored Objects: "), true);

                List<GameObject> IgnoredObjectList = new List<GameObject>();

                this.InitializeIgnoredObjects(IgnoredObjectList);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(SelectedSpawnPositionOption, new GUIContent("Positioning Component: "), StandardLayout);

        if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
            SelectedSpawnPositionOption.intValue = (int)PositioningOptions.Area;

        switch ((PositioningOptions)SelectedSpawnPositionOption.intValue)
        {
            case PositioningOptions.Area:
                {
                    for (int childIndex = 0; childIndex < DynamicSpawned.transform.childCount; childIndex++)
                    {
                        if (DynamicSpawned.transform.GetChild(childIndex).name == "SpawnArea")
                        {
                            DynamicSpawned.SpawnArea = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.SpawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                        }
                    }

                    if (!DynamicSpawned.SpawnArea)
                    {
                        DynamicSpawned.SpawnArea = Instantiate(Resources.Load("SpawnArea", typeof(GameObject))) as GameObject; //Resources.Load<GameObject>("Assets /DynamicSpawningSystem/SpawnArea");
                        DynamicSpawned.SpawnArea.transform.SetParent(DynamicSpawned.transform);
                        DynamicSpawned.SpawnArea.transform.name = "SpawnArea";
                        DynamicSpawned.SpawnArea.transform.localPosition = new Vector3(0, 0, 0);
                        
                        if(DynamicSpawned.SpawnArea.GetComponent<MeshFilter>() == null)
                        {
                            DynamicSpawned.SpawnArea.AddComponent<MeshFilter>();
                        }
                        DynamicSpawned.SpawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                        DynamicSpawned.SpawnArea.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
                    }

                    break;
                }

            case PositioningOptions.Point:
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(ActiveSpawnPoint);
                    EditorGUI.indentLevel--;
                    break;
                }
        }

        EditorGUILayout.PropertyField(TriggerSpawnOverridesLogic, new GUIContent("Trigger spawn overrides logic:"), StandardLayout);

        EditorGUI.EndChangeCheck();

        this.serializedObject.ApplyModifiedProperties();

        serializedObject.Update();


        if (GUI.changed)
            EditorUtility.SetDirty(DynamicSpawned);
    }

    void InitializeIgnoredObjects(List<GameObject> IgnoredObjects)
    {
        for (int ObjectIndex = 0; ObjectIndex < DynamicSpawned.IgnoredObjects.Count; ObjectIndex++)
        {
            if (DynamicSpawned.IgnoredObjects[ObjectIndex] != null)
            {
                if (DynamicSpawned.IgnoredObjects[ObjectIndex].GetComponent<Collider>())
                    IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex]);

                if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent)
                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.GetComponent<Collider>())
                        IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.gameObject);

                for (int ChildrenIndex = 0; ChildrenIndex < DynamicSpawned.IgnoredObjects[ObjectIndex].transform.childCount; ChildrenIndex++)
                {
                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).GetComponent<Collider>())
                        IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).gameObject);
                }

            }
        }

        SpawningFunctions.FrustumIgnoredObjects = IgnoredObjects;
    }
}
#endif