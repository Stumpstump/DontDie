using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PowerUpEditor : Editor
{
    [MenuItem("GameObject/Power-Ups/Speed", false, 0)]
    static void CreateSpeedPowerUp()
    { 
        GameObject NewPowerUp = Instantiate(Resources.Load("SpeedPowerUp") as GameObject, Selection.activeTransform);

        NewPowerUp.name = "Speed Power Up";
    }

    [MenuItem("GameObject/Speed Fields/Speed Up", false, 0)]
    static void CreateSpeedUpField()
    {
        GameObject NewField = Instantiate(Resources.Load("SpeedUpField") as GameObject, Selection.activeTransform);

        NewField.name = "Speed Up Field";
    }
}
