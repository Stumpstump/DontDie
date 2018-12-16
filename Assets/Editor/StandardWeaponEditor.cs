#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StandardGunScript))]
public class StandardWeaponEditor : Editor
{
    SerializedProperty UseDynamicDamage;
    SerializedProperty BaseDamage;
    SerializedProperty DynamicDamage;

    SerializedProperty maxShootingRange;
    SerializedProperty spreadRadius;
    SerializedProperty roundsPerSecond;
    SerializedProperty magazineSize;
    SerializedProperty reloadTime;

    SerializedProperty muzzleFlash;
    SerializedProperty impact;

    SerializedProperty reloadSound;
    SerializedProperty shootingSound;


    void Awake()
    {
        UseDynamicDamage = this.serializedObject.FindProperty("showDynamicDamage");
        BaseDamage = this.serializedObject.FindProperty("damage");
        DynamicDamage = this.serializedObject.FindProperty("dynamicDamage");

        maxShootingRange = this.serializedObject.FindProperty("maxShootingRange");
        spreadRadius = this.serializedObject.FindProperty("spreadRadius");
        roundsPerSecond = this.serializedObject.FindProperty("roundsPerSecond");
        magazineSize = this.serializedObject.FindProperty("magazineSize");
        reloadTime = this.serializedObject.FindProperty("reloadTime");

        muzzleFlash = this.serializedObject.FindProperty("muzzleFlash");
        impact = this.serializedObject.FindProperty("impact");

        reloadSound = this.serializedObject.FindProperty("ReloadSound");
        shootingSound = this.serializedObject.FindProperty("ShootingSound");
    }

    override public void OnInspectorGUI()
    {
        if (UseDynamicDamage == null)
            Awake();

        //  Debug.Log(serializedObject);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(UseDynamicDamage, new GUIContent("Dynamic Damage: "));
        
        if (UseDynamicDamage.boolValue)
        {
            EditorGUILayout.PropertyField(BaseDamage, new GUIContent("Base Damage: "));
            EditorGUILayout.PropertyField(DynamicDamage, new GUIContent("Max Damage: "));
        }
        else
            EditorGUILayout.PropertyField(BaseDamage, new GUIContent("Damage: "));

        EditorGUILayout.PropertyField(maxShootingRange);
        EditorGUILayout.PropertyField(spreadRadius);
        EditorGUILayout.PropertyField(roundsPerSecond);
        EditorGUILayout.PropertyField(magazineSize);
        EditorGUILayout.PropertyField(reloadTime);

        EditorGUILayout.PropertyField(muzzleFlash);
        EditorGUILayout.PropertyField(impact);

        EditorGUILayout.PropertyField(reloadSound);
        EditorGUILayout.PropertyField(shootingSound);


        EditorGUI.EndChangeCheck();

        
        if(this.serializedObject.hasModifiedProperties)
            this.serializedObject.ApplyModifiedProperties();

    }
}

#endif