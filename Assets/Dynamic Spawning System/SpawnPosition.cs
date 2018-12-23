using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace DDS
{
    public class SpawnPosition : SpawningComponent
    {
        
        [SerializeField]
        [Tooltip("Assign all Objects the ground detection should ignore to this mask")]
        public LayerMask IgnoredSpawnObject;

        [SerializeField]
        [Tooltip("Adjust this height to not collide with the roof of the room etc.")]
        public float GroundDetectionHeight;


        void Start()
        {
        }

        /// <summary>
        /// Set FrustumCamera to null if you don't want the Frustum Check.
        /// Returns false if it couldn't allocate the desired amount of positions.
        /// </summary>
        public override bool GetPositions(SpawnAbleObject Object, int AmountOfPosition, Camera FrustumCamera, bool useCharaterController, out Vector3[] ReturnedPosition)
        {
            PersonalLogicScript PersonalScript = Object.ObjectToSpawn.GetComponent<PersonalLogicScript>();
                        
            bool UsePersonalLogic = false;

            if (PersonalScript != null)
            {
                UsePersonalLogic = true;
            }

            ReturnedPosition = new Vector3[1];
            ReturnedPosition[0] = new Vector3();

            ReturnedPosition[0].x = transform.position.x;
            ReturnedPosition[0].z = transform.position.z;

            Bounds ObjectBounds = new Bounds();

            Vector3 CenterOffset = new Vector3(); ;

            if(!useCharaterController)
            {
                ObjectBounds = Object.ObjectToSpawn.GetComponent<Renderer>().bounds;
                CenterOffset = ObjectBounds.center - Object.ObjectToSpawn.transform.position;
            }

            else
            {
                ObjectBounds = Object.ObjectToSpawn.GetComponent<CharacterController>().bounds;
                CenterOffset = ObjectBounds.center - Object.ObjectToSpawn.transform.position;
            }


            if (Object.ApplyLogicToChilds)
            {
                foreach (Renderer renderer in Object.ObjectToSpawn.GetComponentsInChildren<Renderer>())
                {
                    ObjectBounds.Encapsulate(renderer.bounds);
                }
            }


            RaycastHit Hit = new RaycastHit();

            Vector3 BoxCastCenter = new Vector3(transform.position.x, transform.position.y + ObjectBounds.extents.y + GroundDetectionHeight) + CenterOffset;

            Debug.DrawRay(BoxCastCenter + ObjectBounds.extents, Vector3.down);

            if(!Physics.BoxCast(BoxCastCenter, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, GroundDetectionHeight + ObjectBounds.size.y, ~IgnoredSpawnObject))
            {
                Debug.Log("<color=red> No ground detected, please readjust your Spawn Point height </color>");
                return false;
            }

//             if (!Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + ObjectBounds.extents.y + GroundDetectionHeight, transform.position.z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, GroundDetectionHeight + ObjectBounds.extents.y, ~IgnoredSpawnObject))
//             {
// 
//             }

            float Distance = 0;

            if (Hit.point.y < transform.position.y )
            {
                Distance = Hit.point.y  - transform.position.y;

                if (Distance < 0)
                    Distance *= -1;
            }

            ReturnedPosition[0].y = Hit.point.y;

            if (!UsePersonalLogic)
            {
                Collider[] OverlapingColliders = Physics.OverlapBox(new Vector3(transform.position.x, Hit.point.y + ObjectBounds.size.y / 2, transform.position.z) + CenterOffset, ObjectBounds.extents);

                List<Collider> OverlappingColliderList = new List<Collider>(OverlapingColliders);


                for (int a = 0; a < OverlappingColliderList.Count; a++)
                {
                    if (OverlappingColliderList[a].gameObject != transform.gameObject && OverlappingColliderList[a].gameObject != Hit.transform.gameObject)
                    {
                        return false;
                    }

                }

                if (Distance > Object.AdaptableSpawnHeight)
                {
                    return false;
                }                                

                if (FrustumCamera != null)
                {
                    if (SpawningFunctions.IsVisible(FrustumCamera, Object.ObjectToSpawn, ReturnedPosition[0]))
                        return false;

                    else if (Object.ApplyLogicToChilds)
                        if (SpawningFunctions.IsAnyChildVisible(Object.ObjectToSpawn, ReturnedPosition[0], FrustumCamera))
                            return false;
                }
            }

            return true;
        }
    }



}