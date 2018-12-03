using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public EventHandler PowerUps;

        [SerializeField] [Tooltip("In Percent")] private float MouseSensitivity;
        [SerializeField] [Tooltip("In Percent")] private float SprintingSpeedModifier;
        [SerializeField] private float MaxCameraXAngle;
        [SerializeField] private float JumpHeight;
        [SerializeField] private float WindDownDuration;
        [SerializeField] private float Gravity;
        [SerializeField] private float JumpDuration;
        [SerializeField] private float WindUpDuration;
        [SerializeField] private float ForwardSpeed;
        [SerializeField] private float BackwardSpeed;
        [SerializeField] private float StrafeSpeed;
        [SerializeField] private float StickToGroundForce;

        [Tooltip("Time the player needs to fully rest on the x and z axes while jumping/falling")]
        [SerializeField] private float GravityWeight;

        private float SpeedFactor = 100;
        private Camera FirstPersonCamera;
        private CharacterController Controller;
        private PlayerMovementStatus MovementStatus;
        private Vector3 DirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovement = new Vector3();

        private float Speed = 0;
        private float ElapsedSpeedPowerUpTime;
   


        Coroutine WindUpCouroutine;
        Coroutine WindDownCouroutine;
        Coroutine FallCoroutine;
        Coroutine SlidingCoroutine;
        Coroutine JumpingCoroutine;

        // Start is called before the first frame update
        void Awake()
        {
            Controller = GetComponent<CharacterController>();
            FirstPersonCamera = Camera.main;
        }


        // Update is called once per frame
        void Update()
        {        
            //Get the W,A,S and D input and normalize it
            DirectionalMovementInputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            DirectionalMovementInputs.Normalize();

            //Update the active Power Ups
            PowerUps?.Invoke(this, new EventArgs());

            UpdateMovementStatus();
            UpdateSpeed();
            Rotate();

            switch (MovementStatus)
            {
                case PlayerMovementStatus.Walking:
                    {
                        Move();
                        break;
                    }

                case PlayerMovementStatus.Idling:
                    {
                        Move();
                        break;
                    }

            }

            //Check the angle of the objects - move this to a function and add a slide function to it
            RaycastHit[] hits;     
            hits = Physics.BoxCastAll(transform.position + Vector3.up, Controller.bounds.extents, Vector3.down);


            float currentIndex = 0;
            float currentDistance = 1000;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if(hit.transform != this.transform)
                {
                    if(hit.distance <= currentDistance)
                    {
                        currentDistance = hit.distance;
                        currentIndex = i;
                    }
                }
            }
       
        }

        void Move()
        {
            if (WindDownCouroutine != null)
            {
                DirectionalMovementInputs = LastDirectionalMovementInputs;
            }

            LastDirectionalMovementInputs = DirectionalMovementInputs;
            Vector3 Movement = new Vector3();
            Movement = transform.TransformDirection(DirectionalMovementInputs) * Time.deltaTime * Speed * SpeedFactor / 100;
            Movement.y = StickToGroundForce * Time.deltaTime;

            Controller.Move(Movement);
        }

        void Rotate()
        {
            Vector3 CharacterRotation = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
            Vector3 CameraRotation = new Vector3(0, FirstPersonCamera.transform.eulerAngles.y, FirstPersonCamera.transform.eulerAngles.z);

            CharacterRotation.y = transform.eulerAngles.y - Input.GetAxisRaw("Mouse X") * -1 * MouseSensitivity / 100;
            CameraRotation.x = FirstPersonCamera.transform.eulerAngles.x - Input.GetAxisRaw("Mouse Y") * MouseSensitivity / 100;

            if (FirstPersonCamera.transform.eulerAngles.x > MaxCameraXAngle) CameraRotation.x -= 360;

            CameraRotation.x = Mathf.Clamp(CameraRotation.x, -MaxCameraXAngle, MaxCameraXAngle);

            FirstPersonCamera.transform.eulerAngles = CameraRotation;
            transform.eulerAngles = CharacterRotation;
        }

        void Jump()
        {
            if(Controller.isGrounded)
            {
                JumpingCoroutine = StartCoroutine(JumpEvent());
                
            }
        }

        IEnumerator JumpEvent()
        {
            Vector3 MovementTillNow = new Vector3();
            Vector3 Direction = transform.TransformDirection(LastDirectionalMovementInputs) * Speed;
            Direction.y = 0f;

            float elapsedFallingDownTime = 0f;
            float JumpTime = 0f;
            do
            {
                JumpTime += Time.deltaTime;
                Vector3 DirectionThisUpdate = Direction;

                Vector3 MovementThisUpdate = Math.MathParabola.Parabola(Vector3.zero, DirectionThisUpdate, JumpHeight, JumpTime, JumpDuration) - MovementTillNow;

                if(JumpTime / JumpDuration > 0.45 && GravityWeight > 0)
                {                    
                    if (elapsedFallingDownTime > GravityWeight)
                        elapsedFallingDownTime = GravityWeight;

                    MovementThisUpdate.x = Mathf.Lerp(MovementThisUpdate.x, 0, elapsedFallingDownTime / GravityWeight);
                    MovementThisUpdate.y -= Mathf.LerpUnclamped(0, GravityWeight, elapsedFallingDownTime /GravityWeight);
                    MovementThisUpdate.z = Mathf.Lerp(MovementThisUpdate.z, 0, elapsedFallingDownTime / GravityWeight);
                    elapsedFallingDownTime += Time.deltaTime;
                }

                Controller.Move(MovementThisUpdate);

                MovementTillNow += MovementThisUpdate;

                yield return null;

            } while (!Controller.isGrounded && Controller.collisionFlags != CollisionFlags.Above);

            LastDirectionalMovementInputs = new Vector3(0, 0, 0);
            JumpingCoroutine = null;
        }

        IEnumerator FallEvent()
        {
            Vector3 Direction = transform.TransformDirection(LastDirectionalMovementInputs) * Speed;
            Vector3 MovementTillNow = new Vector3();

            float elapsedFallingDownTime = 0f;
            float ElapsedTime = 0f;            
            Direction.y = Gravity;
            do
            {
                Vector3 DirectionThisUpdate = Direction;
                ElapsedTime += Time.deltaTime;

                Vector3 MovementThisUpdate = Math.MathParabola.Parabola(Vector3.zero, DirectionThisUpdate, 0, ElapsedTime, JumpDuration) - MovementTillNow;

                if (ElapsedTime / JumpDuration > 0.45 && GravityWeight != 0)
                {

                    if (elapsedFallingDownTime >= GravityWeight)
                        elapsedFallingDownTime = GravityWeight;

                    MovementThisUpdate.x = Mathf.Lerp(MovementThisUpdate.x, 0, elapsedFallingDownTime / GravityWeight);
                    MovementThisUpdate.z = Mathf.Lerp(MovementThisUpdate.z, 0, elapsedFallingDownTime / GravityWeight);
                    elapsedFallingDownTime += Time.deltaTime;
                }

                MovementThisUpdate.y += Mathf.LerpUnclamped(0, Gravity, ElapsedTime/JumpDuration);

                Controller.Move(MovementThisUpdate);
                MovementTillNow += MovementThisUpdate;
                yield return null;
            } while (!Controller.isGrounded && Controller.collisionFlags != CollisionFlags.Above);

            FallCoroutine = null;
        }
        

        void UpdateSpeed()
        {
            if (JumpingCoroutine != null|| FallCoroutine != null)
                return;

            float WalkingSpeed = GetWalkingSpeed();
            float SprintingSpeed = WalkingSpeed / 100 * SprintingSpeedModifier;
            float DesiredSpeed = !Input.GetKey(KeyCode.LeftShift) ? WalkingSpeed : SprintingSpeed;

            if (WindUpCouroutine == null && WindDownCouroutine == null)
                Speed = DesiredSpeed;
        }

        bool isMovementInputZero()
        {
              return DirectionalMovementInputs.x != 0 || DirectionalMovementInputs.z != 0;        
        }

        float GetWalkingSpeed()
        {
            if(isMovementInputZero())
            {
                if (DirectionalMovementInputs.z > 0)
                    return ForwardSpeed;

                else if (DirectionalMovementInputs.z < 0)
                    return BackwardSpeed;

                else return StrafeSpeed;
            }

            else
            {
                if (LastDirectionalMovementInputs.z > 0)
                    return ForwardSpeed;

                else if (LastDirectionalMovementInputs.z < 0)
                    return BackwardSpeed;

                else if (LastDirectionalMovementInputs.x > 0 || LastDirectionalMovementInputs.x < 0)
                    return StrafeSpeed;

                else return 0;
            }
        }

        void UpdateMovementStatus()        
        {
            if (JumpingCoroutine != null|| FallCoroutine != null || SlidingCoroutine != null) return;

            else if(!Controller.isGrounded && Controller.collisionFlags != CollisionFlags.Above && FallCoroutine == null)
            {
                FallCoroutine = StartCoroutine(FallEvent());
                MovementStatus = PlayerMovementStatus.Falling;
                if(WindDownCouroutine != null)
                {
                    StopCoroutine(WindDownCouroutine);
                    WindDownCouroutine = null;
                }
            }

            else if (Input.GetKeyDown(KeyCode.Space))
            {
                MovementStatus = PlayerMovementStatus.Jumping;

                if (WindDownCouroutine != null)
                {
                    StopCoroutine(WindDownCouroutine);
                    WindDownCouroutine = null;
                }

                if(WindUpCouroutine != null)
                {
                    StopCoroutine(WindUpCouroutine);
                    WindUpCouroutine = null;
                }
                Jump();
                return;
            }

            
            else if(!isMovementInputZero())
            {
                if (WindDownCouroutine == null && Speed > 0)
                {
                    WindDownCouroutine = StartCoroutine(WindDown());                
                }

                if (Speed == 0)
                    MovementStatus = PlayerMovementStatus.Idling;
                else
                    MovementStatus = PlayerMovementStatus.Walking;

                if (WindUpCouroutine != null)
                {
                    StopCoroutine(WindUpCouroutine);
                    WindUpCouroutine = null;
                }

            }

            else if(isMovementInputZero())
            {
                if (WindDownCouroutine != null)
                {
                    StopCoroutine(WindDownCouroutine);
                    WindDownCouroutine = null;
                }

                if (Speed > 0)
                    MovementStatus = PlayerMovementStatus.Walking;

                if (Speed == 0 && WindUpCouroutine == null)
                    WindUpCouroutine = StartCoroutine(WindUp());                    
            }
            
        }

        public void ChangeSpeedFactor(int amountToChange)
        {
            SpeedFactor += amountToChange;            
        }

        IEnumerator WindUp()
        {
           
            float WindUpCounter = 0f;
            float WalkingSpeed = GetWalkingSpeed();
            float SprintingSpeed = WalkingSpeed / 100 * SprintingSpeedModifier;
            float DesiredSpeed = !Input.GetKey(KeyCode.LeftShift) ? WalkingSpeed : SprintingSpeed;
            float StartingSpeed = Speed;

            while (WindUpCounter < WindUpDuration)
            {
                WindUpCounter += Time.deltaTime;
                Speed = Mathf.Lerp(StartingSpeed, DesiredSpeed, WindUpCounter/WindUpDuration);
                yield return null;
            }

            Speed = DesiredSpeed;
            WindUpCouroutine = null;
        }

        IEnumerator WindDown()
        {
            float WindDownCounter = 0f;
            float StartingSpeed = Speed;
            while(WindDownCounter < WindDownDuration)
            {
                float WalkingSpeed = GetWalkingSpeed();
                float SprintingSpeed = WalkingSpeed / 100 * SprintingSpeedModifier;
                float DesiredSpeed = !Input.GetKey(KeyCode.LeftShift) ? WalkingSpeed : SprintingSpeed;
                WindDownCounter += Time.deltaTime;
                Speed = Mathf.Lerp(StartingSpeed, 0, WindDownCounter/WindDownDuration);
                yield return null;
            }

            Speed = 0f;
            WindDownCouroutine = null;
        }

        IEnumerator Slide()
        {
            float StartingSpeed = Speed;
            yield return null;
        }
      
    }

}
