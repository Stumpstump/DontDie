using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


namespace DDS
{
    public class Spawner : MonoBehaviour
    {
        delegate GameObject[] SpawningFunction(Component PositioningComponent, Camera FrustumCamera);

        SpawningFunction SelectedSpawningFunction;

        [SerializeField]
        public ContiniousWaveStatus CurrentContiniousWaveStatus;

        [SerializeField]
        private int SelectedSpawnPosition;

        [SerializeField]
        public int WaveSpawnAmount;

        [SerializeField]
        public bool UseOcclusionCulling;

        [SerializeField]
        public List<GameObject> IgnoredObjects = new List<GameObject>();

        [SerializeField]
        public bool TriggerSpawn;

        [SerializeField]
        public bool ShowIgnoredObjects;

        [SerializeField]
        public bool TriggerSpawnOverridesLogic;

        [SerializeField]
        public int MaximalSpawnedObjectsAlive;

        [SerializeField]
        public float SpawnDelay;

        [SerializeField]
        public float RangeToCheck;

        [SerializeField]
        public bool DoSpawnIfNotInRange;

        [SerializeField]
        public bool DoSpawnContinuousWaves;

        [SerializeField]
        public bool DoSpawnInFrustum;

        [SerializeField]
        public bool DoLimitObjectsAlive;

        [SerializeField]
        public bool IsNotInRange;

        [SerializeField]
        public SpawnedObjectContainer SpawnedObjects = new SpawnedObjectContainer();

        [SerializeField]
        public List<SpawningComponent> SpawnPositions;

        [SerializeField]
        public GameObject Player;

        [SerializeField]
        public GameObject SpawnArea;

        [SerializeField]
        public PositioningOptions SelectedSpawnPositionOption;

        [SerializeField]
        public SpawningStyles SelectedSpawningStyle;

        [SerializeField]
        public IdentifyPlayer SelectedPlayerIdentification;

        [SerializeField]
        public DistanceCheckingStyles SelectedDistanceCheck;

        [SerializeField]
        public Identification PlayerIdentificationData;

        [SerializeField]
        public Camera FrustumCamera;

        private float SpawnInterval;

        private SpawningComponent PositioningComponent = null;

        void Awake()
        {
            this.InitializeSpawnPositions();
            this.InitializeObjectToCheck();

            SpawningFunctions.WaveSpawnAmount = WaveSpawnAmount;
        }

        void Start()
        {

        }

        void Update()
        {           
            SpawningFunctions.TriggerSpawnOverridesLogic = TriggerSpawnOverridesLogic;
            SpawningFunctions.IsTriggerSpawn = TriggerSpawn;           
            SpawningFunctions.UseOcclusionCulling = UseOcclusionCulling;          

            SpawnedObjects.Update();
            
            if(DoSpawnIfNotInRange)
                this.UpdateDistance();

            SpawnInterval += Time.deltaTime;

            PositioningComponent = null;

            switch (SelectedSpawningStyle)
            {
                case SpawningStyles.Wave:
                    PositioningComponent = GetComponentInChildren<SpawnArea>();
                    break;

                case SpawningStyles.Continuous:
                    if (SelectedSpawnPositionOption == PositioningOptions.Area)
                    {
                        PositioningComponent = GetComponentInChildren<SpawnArea>();
                    }

                    else
                    {
                        PositioningComponent = SpawnPositions[SelectedSpawnPosition];
                    }
                    break;
            }

            Camera camera = null;

            if (!DoSpawnInFrustum)
                camera = FrustumCamera;

            if (IsSpawningAllowed())
            {
                TriggerSpawn = false;
                GameObject[] ReturnedObjects = SpawningFunctions.Spawn(PositioningComponent, camera, SelectedSpawningStyle);
                if (ReturnedObjects != null)
                {
                    SpawnedObjects.AddObjects(ReturnedObjects);
                    SpawnInterval = 0f;
                }
            }
        }

        /// <summary>
        /// Goes through the spawner logic to check if spawning is at this moment allowed.
        /// </summary>
        /// <returns></returns>
        bool IsSpawningAllowed()
        {
            if(!TriggerSpawnOverridesLogic || (!TriggerSpawn && TriggerSpawnOverridesLogic))
            {
                int DesiredObjectAmount = 1;

                if(SelectedSpawningStyle == SpawningStyles.Wave)
                    DesiredObjectAmount = SpawningFunctions.WaveSpawnAmount;
            
                if(SelectedSpawningStyle == SpawningStyles.Wave && DoSpawnContinuousWaves)
                {
                    if (SpawnedObjects.Size > 0 || CurrentContiniousWaveStatus == ContiniousWaveStatus.Stopped || DoSpawnIfNotInRange && !IsNotInRange)
                        return false;
                }         

                else if(SpawnInterval < SpawnDelay || DoSpawnIfNotInRange && !IsNotInRange || (DoLimitObjectsAlive && MaximalSpawnedObjectsAlive < DesiredObjectAmount + SpawnedObjects.Size))            
                    return false;
            }
            return true;            
        }

        /// <summary>
        /// Updates the boolean IsNotInRange based on the distance of the PlayerObject and the RangeTocheck.
        /// </summary>
        void UpdateDistance()
        {
            if (DoSpawnIfNotInRange && Player)
            {
                switch (SelectedDistanceCheck)
                {
                    case DistanceCheckingStyles.TwoDimensionalCheck:
                        IsNotInRange = DistanceChecking.TwoDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;

                    case DistanceCheckingStyles.ThreeDimensionalCheck:
                        IsNotInRange = DistanceChecking.ThreeDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;
                }
            }
        }

        /// <summary>
        /// Used if the DistanceCheckingStyle is set to SphereCollider to check if the PlayerObject is in the sphere. 
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerEnter(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    IsNotInRange = false;
        }

        /// <summary>
        /// Used if the DistanceCheckingStyle is set to SphereCollider to check if the PlayerObject left the sphere. 
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerExit(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    IsNotInRange = true;

        }

        /// <summary>
        /// Initializes the PlayerObject for the range check.
        /// </summary>
        public void InitializeObjectToCheck()
        {
            switch (SelectedPlayerIdentification)
            {
                case IdentifyPlayer.byField:
                    Player = PlayerIdentificationData.Object;
                    break;

                case IdentifyPlayer.byName:
                    Player = GameObject.Find(PlayerIdentificationData.Name);
                    break;

                case IdentifyPlayer.byTag:
                    #if UNITY_EDITOR
                    string Tag = UnityEditorInternal.InternalEditorUtility.tags[PlayerIdentificationData.Tag];
                    Player = GameObject.FindWithTag(Tag);
                    #endif
                    break;
            }
        }

        /// <summary>
        /// Initializes the ComponentList SpawnPositions with every child which contains the SpawnPosition component.
        /// </summary>
        public void InitializeSpawnPositions()
        {
            SpawnPositions = new List<SpawningComponent>();

            foreach (var Child in transform.GetComponentsInChildren<SpawnPosition>())
            {
                SpawnPositions.Add(Child);
            }
        }     

        /// <summary>
        /// Use this to change the currently selected SpawnPosition by index.
        /// </summary>
        /// <param name="PositionToSet"></param>
        public void SetSpawnPosition(int PositionToSet)
        {
            try
            {
                if(!SpawnPositions[PositionToSet])
                {
                    SelectedSpawnPosition = PositionToSet;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.StackTrace + "" + "Position to set was out of bounds!");                
            }
        }

        /// <summary>
        /// Use this to change the currently selected SpawnPosition by the name.
        /// </summary>
        /// <param name="PositionName"></param>
        public bool SetSpawnPosition(string PositionName)
        {
            for(int i = 0; i < SpawnPositions.Count; i++)
            {
                if(SpawnPositions[i].name == PositionName)
                {
                    SelectedSpawnPosition = i;
                    return true;
                }                
            }

            Debug.LogError("Position with the name " + PositionName + " couldn't be found!");
            return false;
        }
    }



 
}
