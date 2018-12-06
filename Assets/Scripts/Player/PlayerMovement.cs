using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public EventHandler PowerUps;

        [SerializeField] private PlayerCamera FirstPersonCamera;
        [SerializeField] [Tooltip("In Percent")] private float SprintingSpeedModifier;
        [SerializeField] private float JumpHeight;
        [SerializeField] private float WindDownDuration;
        [SerializeField] private float Gravity;
        [SerializeField] private float JumpDuration;
        [SerializeField] private float WindUpDuration;
        [SerializeField] private float ForwardSpeed;
        [SerializeField] private float BackwardSpeed;
        [SerializeField] private float StrafeSpeed;
        [SerializeField] private float StickToGroundForce;
        [SerializeField] private float SlideGravityAmplifier;

        [Tooltip("Time the player needs to fully rest on the x and z axes while jumping/falling")]
        [SerializeField] private float GravityWeight;

        private float SpeedFactor = 100;
        private CharacterController Controller;
        private PlayerMovementStatus MovementStatus;
        private Vector3 DirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovement = new Vector3();

        private float Speed = 0;
        private float ElapsedSpeedPowerUpTime;
        private float SlideAngle;
        private Vector3 SlideNormal;

        private Vector3 LastCollisionDistance = new Vector3();
        private Collider coll = new Collider();

        Coroutine WindUpCouroutine;
        Coroutine WindDownCouroutine;
        Coroutine FallCoroutine;
        Coroutine SlidingCoroutine;
        Coroutine JumpingCoroutine;

        // Start is called before the first frame update
        void Awake()
        {
            Controller = GetComponent<CharacterController>();
            FirstPersonCamera.Initialize(transform);
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
            FirstPersonCamera.Rotate();

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

        void Jump()
        {
            JumpingCoroutine = StartCoroutine(JumpEvent());
 
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

            else if(Controller.isGrounded && ShouldSlide())
            {
                SlidingCoroutine = StartCoroutine(Slide());
            
            }

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


            else if (Input.GetKeyDown(KeyCode.Space) && Controller.isGrounded)
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
            MovementStatus = PlayerMovementStatus.Sliding;
            float StartingSpeed = Speed *SpeedFactor / 100;
            float elapsedTime = 0f;
            Vector3 slideNormal = SlideNormal;
            do 
            {
                Vector3 MoveThisUpdate = SlideNormal;
                MoveThisUpdate.y = Gravity * SlideGravityAmplifier;
                elapsedTime += Time.deltaTime;
                Controller.Move(MoveThisUpdate * Time.deltaTime * StartingSpeed);

                yield return null;

            } while (ShouldSlide()) ;

            SlidingCoroutine = null;
            LastDirectionalMovementInputs = new Vector3(0, 0, 0);
            UpdateMovementStatus();
        }
      
        bool ShouldSlide()
        {
            if(Vector3.Angle(Vector3.up, SlideNormal) > Controller.slopeLimit && Vector3.Angle(Vector3.up, SlideNormal) < 90)
            {
                Vector3 p1 = Controller.transform.position + Controller.center + Vector3.up * -Controller.height * 0.5f;
                Vector3 p2 = p1 + Vector3.up * Controller.height;
                RaycastHit[] colliders = Physics.CapsuleCastAll(p1, p2, Controller.radius - 0.3f, transform.up * -1, 100);
                {
                    foreach(var collider in colliders)
                    {
                        if(collider.collider == coll)
                        {
                            return true;
                        }
                    }                    

                }

            }

          

//             //Check the angle of the objects that got hit and take the one with the smallest distance to us
//             RaycastHit[] hits;
//             hits = Physics.BoxCastAll(transform.position + Vector3.up, Controller.bounds.extents - new Vector3(Controller.bounds.extents.x
//                  * 0.8f, Controller.bounds.extents.y * 0.8f, Controller.bounds.extents.z * 0.8f), Vector3.down);
// 
//             if (hits.Length > 0)
//             {
//                 var hit = hits.OrderBy(i => i.distance).First();
//                 float Angle = Vector3.Angle(Vector3.up, hit.normal);
//                 if (Angle > Controller.slopeLimit)
//                 {
//                     SlideNormal = hit.normal;
//                     SlideAngle = Angle;
//                     return true;
//                 }
//             }

            return false;
        }

        void OnControllerColliderHit (ControllerColliderHit hit)
        {

            Ray ray = new Ray(transform.position, hit.point- transform.position);
            RaycastHit rayHit;
            if(hit.collider.Raycast(ray, out rayHit, 10f))
            {
            //    if(Vector3.Distance(rayHit.point, transform.position) < Vector3.Distance(LastCollisionDistance, transform.position))
            //    {
                    LastCollisionDistance = rayHit.point;
                    SlideNormal = rayHit.normal;
                    coll = rayHit.collider;
           //     }
            }

        }

    }

}
