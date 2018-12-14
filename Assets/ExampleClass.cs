//Attach this script to your Camera
//This draws a line in the Scene view going through a point 200 pixels from the lower-left corner of the screen
//To see this, enter Play Mode and switch to the Scene tab. Zoom into your Camera's position.
using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    Camera cam;
    public GameObject recticle;
    public float SpreadRadius;

//     void Start()
//     {
//         cam = GetComponent<Camera>();
//     }
// 
//     void Update()
//     {
//         float theta = Random.Range(0f, 1f) * 2 * Mathf.PI;
//         float r = SpreadRadius * Mathf.Sqrt(Random.Range(0f, 1f));
// 
//         Ray ray = cam.ScreenPointToRay(recticle.transform.position);
//         ray.direction += transform.TransformDirection(new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 1));
// 
//         Debug.DrawRay(ray.origin, ray.direction.normalized * 10, Color.yellow);
//     }
}