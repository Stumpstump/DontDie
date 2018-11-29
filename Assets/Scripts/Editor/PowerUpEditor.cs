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
}
