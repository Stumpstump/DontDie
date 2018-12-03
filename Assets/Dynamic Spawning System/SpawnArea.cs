﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace DDS
{ 
    public class SpawnArea : SpawningComponent
    {
       [SerializeField]
        [Tooltip("Assign all Objects the ground detection should ignore to this mask")]
        public LayerMask IgnoredSpawnObject;

        [SerializeField]
        [Tooltip("Adjust this height to not collide with the roof of the room, etc.")]
        public float GroundDetectionHeight;

        /// <summary>
        /// Set FrustumCamera to null if you don't want the Frustum Check.
        /// Returns false if it couldn't allocate the desired amount of positions.
        /// </summary>
        public override bool GetPositions(SpawnAbleObject Object, int DesiredAmountOfPositions, Camera FrustumCamera, out Vector3[] ReturnedPositions)
        {

            PersonalLogicScript PersonalScript = Object.ObjectToSpawn.GetComponent<PersonalLogicScript>();

            bool UsePersonalLogic = false;

            if (PersonalScript != null)
            {
                UsePersonalLogic = true;
            }


            Bounds ObjectBounds = Object.ObjectToSpawn.GetComponent<Renderer>().bounds;

            if (Object.ApplyLogicToChilds)
            {
                foreach (Renderer renderer in Object.ObjectToSpawn.GetComponentsInChildren<Renderer>())
                {
                    ObjectBounds.Encapsulate(renderer.bounds);
                }
            }

            ObjectBounds.center = ObjectBounds.center - Object.ObjectToSpawn.transform.position;

            float AreaWidth, AreaLength, ObjectWidth, ObjectLength;

            ReturnedPositions = new Vector3[0];

            Vector3 AreaTopRightPosition = new Vector3();

            AreaTopRightPosition.x = transform.position.x - GetComponent<MeshCollider>().bounds.extents.x;
            AreaTopRightPosition.z = transform.position.z - GetComponent<MeshCollider>().bounds.extents.z;

            AreaWidth = GetComponent<MeshCollider>().bounds.size.x;
            AreaLength = GetComponent<MeshCollider>().bounds.size.z;


            ObjectWidth = ObjectBounds.size.x;
            ObjectLength = ObjectBounds.size.z;


            float ContainableSizeWidth = (AreaWidth / ObjectWidth);
            float ContainableSizeHeight = (AreaLength / ObjectLength);


            int ContainableAreaSize = (int)(ContainableSizeHeight * ContainableSizeWidth);

            Vector3[] Positions = new Vector3[ContainableAreaSize];

            Vector3 LastPosition = new Vector3();

            LastPosition = AreaTopRightPosition;

            int CurrentRow = 1, CurrentColumn = 1;

            Positions[0] = LastPosition;

            for (int i = 1; i < ContainableAreaSize; i++)
            {
                if(CurrentColumn > (int)ContainableSizeWidth)
                {
                    CurrentColumn = 1;
                    CurrentRow += 1; 
                }

                Vector3 CurrentPosition = new Vector3();


                if(CurrentColumn == 1)
                {
                    CurrentPosition.x = AreaTopRightPosition.x + ObjectWidth / 2;
                }

                else
                {
                    CurrentPosition.x = AreaTopRightPosition.x + ObjectWidth / 2 + (CurrentColumn - 1) * ObjectWidth;               
                }

                if(CurrentRow == 1)
                {
                    CurrentPosition.z = AreaTopRightPosition.z + ObjectLength / 2;
                }

                else
                {
                    CurrentPosition.z = AreaTopRightPosition.z + ObjectLength / 2 + (CurrentRow - 1) * ObjectLength;
                }

                CurrentColumn++;

                Positions[i] = CurrentPosition;
            }

            List<Vector3> SpawnAblePositions = new List<Vector3>();

            List<Vector3> IndexOfObjectsToRemove = new List<Vector3>();


            for (int i = 0; i < Positions.Length; i++)
            {
                RaycastHit Hit;

                if (!Physics.BoxCast(new Vector3(Positions[i].x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2  + GroundDetectionHeight, Positions[i].z), ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, GroundDetectionHeight + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2, ~IgnoredSpawnObject, QueryTriggerInteraction.Ignore))
                {
                    Debug.Log("<color=red> No ground detected, please readjust your Spawn Area height </color>");
                    return false;
                }

                float Distance = 0;

                if (Hit.point.y + ObjectBounds.size.y / 2 > transform.position.y + ObjectBounds.size.y / 2)
                {
                    Distance = Hit.point.y + ObjectBounds.size.y / 2 - transform.position.y + ObjectBounds.size.y / 2;
                
                    if (Distance < 0)
                        Distance *= -1;
                }

                if(!UsePersonalLogic)
                {
                    Collider[] OverlapingColliders = Physics.OverlapBox(new Vector3(Positions[i].x, Hit.point.y + ObjectBounds.size.y / 2, Positions[i].z), ObjectBounds.extents);

                    List<Collider> OverlappingColliderList = new List<Collider>(OverlapingColliders);

                    bool DoDeletePosition = false;

                    for (int a = 0; a < OverlappingColliderList.Count; a++)
                    {
                        if (OverlappingColliderList[a].gameObject != transform.gameObject && OverlappingColliderList[a].gameObject != Hit.transform.gameObject)
                        {
                            DoDeletePosition = true;
                        }

                    }

                    if (Distance < Object.AdaptableSpawnHeight && !DoDeletePosition)
                    {
                        Positions[i].y = Hit.point.y + ObjectBounds.size.y / 2;
                        SpawnAblePositions.Add(Positions[i]);
                    }

                }

                else
                {
                    Positions[i].y = Hit.point.y + ObjectBounds.size.y / 2;
                    SpawnAblePositions.Add(Positions[i]);
                }

            }
            
            if(!UsePersonalLogic)
            {
                for (int i = 0; i < SpawnAblePositions.Count; i++)
                {
                    if (FrustumCamera != null)
                    {
                        if (SpawningFunctions.IsVisible(FrustumCamera, Object.ObjectToSpawn, SpawnAblePositions[i]))
                            IndexOfObjectsToRemove.Add(SpawnAblePositions[i]);


                        else if (Object.ApplyLogicToChilds)
                            if (SpawningFunctions.IsAnyChildVisible(Object.ObjectToSpawn, SpawnAblePositions[i], FrustumCamera))
                                IndexOfObjectsToRemove.Add(SpawnAblePositions[i]);
                    }
                }


                for (int i = 0; i < IndexOfObjectsToRemove.Count; i++)
                {
                    SpawnAblePositions.Remove(IndexOfObjectsToRemove[i]);
                }
            }

            if (SpawnAblePositions.Count < DesiredAmountOfPositions)
                return false;

            int MaxLoops = DesiredAmountOfPositions * 2;

            int Loop = 0;

            List<Vector3> BufferList = new List<Vector3>();

            for (int i = 0; i < DesiredAmountOfPositions; i++)
            {
                Loop++;

                if (Loop > MaxLoops)
                    return false;

                int SelectedPosition = UnityEngine.Random.Range(0, SpawnAblePositions.Count - 1);

                bool AlreadyUsed = false;

                foreach (Vector3 Position in BufferList)
                {
                    if(Position == SpawnAblePositions[SelectedPosition])
                    {
                        AlreadyUsed = true;
                    }
                }

                if (AlreadyUsed == true)
                {
                    i--;
                }

                else
                {
                    BufferList.Add(SpawnAblePositions[SelectedPosition]);
                }
            }


            BufferList.ToArray();

            ReturnedPositions = BufferList.ToArray();

            if (ReturnedPositions.Length < DesiredAmountOfPositions)
            {
                Debug.Log("Spawn Area couldnt find enough positions to spawn!");
                return false;
            }

            return true;
        }

    }



}

