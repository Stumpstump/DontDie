using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DDS;
using Combat;

namespace Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        public EventHandler PowerUps;
        public PlayerMovementStatus MovementStatus;

        [SerializeField] private PlayerCamera FirstPersonCamera;

        [Header("Speed")]
        [SerializeField] private float ForwardSpeed;
        [SerializeField] private float BackwardSpeed;
        [SerializeField] private float StrafeSpeed;
        [SerializeField] private float WindUpDuration;
        [SerializeField] private float WindDownDuration;
        [SerializeField] [Tooltip("In Percent")] private float SprintingSpeedModifier;

        [Header("Gravity")]
        [SerializeField] private float StickToGroundForce;
        [SerializeField] private float SlideGravityAmplifier;
        [SerializeField] private float Gravity;

        [Header("Climbing")]
        [SerializeField] private float MaxClimbHeight;
        [SerializeField] private float MaxClimbDistance;
        [SerializeField] private float ClimbingDuration;

        [Header("Jumping")]
        [SerializeField] private float JumpHeight;
        [SerializeField] private float JumpDuration;
        [Tooltip("Time the player needs to fully rest on the x and z axes while jumping/falling")]
        [SerializeField] private float GravityWeight;

        private float SpeedFactor = 100;
        private StandardGunScript CurrentGun;
        private CharacterController Controller;
        private Health playerHealth;
        private Inventory inventory;

        private Vector3 DirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovement = new Vector3();

        private float Speed = 0;
        private float ElapsedSpeedPowerUpTime;
        private float SlideAngle;
        private float CurrentDirection;
        private Vector3 SlideNormal;
        private float stepOffsset;

        private Vector3 LastCollisionDistance = new Vector3();
        private Collider coll = new Collider();

        private Animator animator;

        float AnimationSpeed;

        Coroutine WindUpCouroutine;
        Coroutine WindDownCouroutine;
        Coroutine FallCoroutine;
        Coroutine SlidingCoroutine;
        Coroutine JumpingCoroutine;
        Coroutine ClimbCoroutine;
        Coroutine BleedOutCoroutine;

        private Vector3 SizeOfTestBox;
        private Vector3 PositionOfTestBox;

        private bool canShoot
        {
            get
            {
                return MovementStatus != PlayerMovementStatus.BleedingOut && MovementStatus != PlayerMovementStatus.Dead && MovementStatus != PlayerMovementStatus.Climbing;
            }
        }

        private bool canMove
        {
            get
            {
                return MovementStatus == PlayerMovementStatus.Walking || MovementStatus == PlayerMovementStatus.Idling;
            }
        }

        private bool canRotate
        {
            get
            {
                return MovementStatus != PlayerMovementStatus.Dead && MovementStatus != PlayerMovementStatus.Climbing;
            }
        }

        private bool canClimb
        {
            get
            {
                if (MovementStatus == PlayerMovementStatus.Falling || MovementStatus == PlayerMovementStatus.Jumping)
                {
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        Vector3 p1 = Controller.transform.position + Controller.center;
                        Vector3 p2 = p1 + Vector3.up * Controller.height;
                        RaycastHit hit;
                        if (Physics.CapsuleCast(p1, p2, Controller.radius, transform.forward, out hit, MaxClimbDistance))
                        {
                            if ((int)Vector3.Angle(Vector3.up, hit.normal) == 90)
                            {
                                RaycastHit HeightCast;
                                if (Physics.Raycast(new Ray(hit.point + new Vector3(0, MaxClimbHeight, 0), Vector3.down), out HeightCast, MaxClimbHeight))
                                {
                                    //Draw the gizmo and use it for the overlapbox
                                    PositionOfTestBox = hit.point;// + transform.forward * Controller.bounds.extents.z / 2;
                                    PositionOfTestBox.y = HeightCast.point.y + Controller.bounds.extents.y;

                                    Collider[] colliders = Physics.OverlapBox(PositionOfTestBox, Controller.bounds.extents, transform.rotation);
                                    bool canClimb = true;
                                    foreach (var col in colliders)
                                    {
                                        if (col.gameObject != this.gameObject && col.gameObject != hit.transform.gameObject)
                                        {
                                            canClimb = false;
                                        }
                                    }

                                    return canClimb;
                                }
                            }
                        }
                    }                         
                }
                return false;                       
            }
        }


        // Start is called before the first frame update
        void Awake()
        {
            Controller = GetComponent<CharacterController>();
            stepOffsset = Controller.stepOffset;
            FirstPersonCamera.Initialize(transform);
            animator = GetComponent<Animator>();
            playerHealth = GetComponent<Health>();
            inventory = GetComponentInChildren<Inventory>();
            playerHealth.dyingEvent += onEnterBleedingOutState;
           
        }

        void OnEnabled()
        {
            Controller = GetComponent<CharacterController>();
        }


        // Update is called once per frame
        void Update()
        {
            if (MovementStatus == PlayerMovementStatus.BleedingOut || MovementStatus == PlayerMovementStatus.Dead) return;

            //Get the W,A,S and D input and normalize it
            DirectionalMovementInputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            DirectionalMovementInputs.Normalize();

            //Update the active Power Ups
            PowerUps?.Invoke(this, new EventArgs());

            UpdateMovementStatus();
            UpdateSpeed();

            CurrentGun = GetComponentInChildren<WeaponSwitching>().GetActiveWeapon().GetComponent<StandardGunScript>();

            if(canShoot)
                if (Input.GetAxisRaw("Fire1") > 0 && CurrentGun != null)
                    CurrentGun.Fire(FirstPersonCamera);

            if (canMove)
                Move();

            if (canRotate)
                FirstPersonCamera.Rotate();

            if(canClimb)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Climb");
                    animator.ResetTrigger("Jump");
                    animator.ResetTrigger("Fall");
                }
                ClimbCoroutine = StartCoroutine(ClimbEvent());
                MovementStatus = PlayerMovementStatus.Climbing;
            }
     
            UpdateAnimationController();
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
            if (animator != null)
            {
                animator.SetTrigger("Jump");                
            }

            JumpingCoroutine = StartCoroutine(JumpEvent());
            Controller.stepOffset = 0.01f;
        }
   
        void UpdateAnimationController()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", Speed);
            animator.SetFloat("Direction", CurrentDirection);
            animator.SetBool("isGrounded", Controller.isGrounded);            
        }

        void ResetCoroutines()
        {
            FallCoroutine = null;
            JumpingCoroutine = null;
            SlidingCoroutine = null;
            ClimbCoroutine = null;
            BleedOutCoroutine = null;
            StopAllCoroutines();
        }

        void onEnterBleedingOutState(object sender, EventArgs e)
        {
            if(MovementStatus != PlayerMovementStatus.BleedingOut && MovementStatus != PlayerMovementStatus.Dead)
            {
                ResetCoroutines();                
                BleedOutCoroutine = StartCoroutine(DyingEvent());
            }
            
        }

        IEnumerator DyingEvent()
        {
            float elapsedTime = 0f;

            MovementStatus = PlayerMovementStatus.BleedingOut;

            //To do: Go in bleeding out animation
            //TIme in which the player can shoot/revived etc
            while (elapsedTime < playerHealth.TimeInBleedingOutState)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            MovementStatus = PlayerMovementStatus.Dead;
            elapsedTime = 0f;

            //To do: Go in dead animation            
            //Maybe go in third person or something like that so you can rotate around your dead body. 
            while (elapsedTime < playerHealth.TimeInDeadState)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            MovementStatus = PlayerMovementStatus.Default;
            playerHealth.Reset();
            inventory.Reset();
            //To do: Drop all weapons from the player (fist exluded) 
            
        }        

        IEnumerator ClimbEvent()
        {
            float elapsedTime = 0f;
            JumpingCoroutine = null;
            FallCoroutine = null;
            Vector3 StartPosition = transform.position;
            
            //Controller.detectCollisions = false;
            Controller.enabled = false;

            while(elapsedTime < ClimbingDuration)
            {
                elapsedTime += Time.deltaTime;
                Vector3 PositionThisTick = Vector3.Lerp(StartPosition, PositionOfTestBox, elapsedTime / ClimbingDuration);


                transform.position = PositionThisTick;                
                yield return null;
            }

            Controller.enabled = true;
            ClimbCoroutine = null;
            UpdateMovementStatus();
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

                if (Controller.collisionFlags != CollisionFlags.Above && JumpDuration < 0.45f)
                    JumpDuration = 0.5f;

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

            } while (!Controller.isGrounded);

            Controller.stepOffset = stepOffsset;
            //LastDirectionalMovementInputs = new Vector3(0, 0, 0);
            JumpingCoroutine = null;
        }

        IEnumerator FallEvent()
        {
            if (animator != null)
                animator.SetTrigger("Fall");

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
            if (isMovementInputZero())
            {
                if (DirectionalMovementInputs.z > 0)
                {
                    CurrentDirection = 0;
                    return ForwardSpeed;
                }

                else if (DirectionalMovementInputs.z < 0)
                {
                    CurrentDirection = 2;
                    return BackwardSpeed;
                }

                else
                {
                    CurrentDirection = DirectionalMovementInputs.x > 0 ? 1 : -1;
                    return StrafeSpeed;
                }
            }

            else
            {
                if (LastDirectionalMovementInputs.z > 0)
                {
                    CurrentDirection = 0;
                    return ForwardSpeed;
                }

                else if (LastDirectionalMovementInputs.z < 0)
                {
                    CurrentDirection = 2;
                    return BackwardSpeed;
                }

                else if (LastDirectionalMovementInputs.x != 0)
                {
                    CurrentDirection = LastDirectionalMovementInputs.x > 0 ? 1 : -1;
                    return StrafeSpeed;
                }

                else return 0;
            }
        }

        void UpdateMovementStatus()        
        {
            if (JumpingCoroutine != null|| FallCoroutine != null || SlidingCoroutine != null || ClimbCoroutine != null) return;

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
                    if(MovementStatus == PlayerMovementStatus.Jumping)
                    {
                        MovementStatus = PlayerMovementStatus.Idling;
                        Speed = 0f;
                    }

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
                RaycastHit colliders; if( Physics.CapsuleCast(p1, p2, Controller.radius - 0.3f, transform.up * -1, out colliders));
                {

                    if (colliders.collider == coll)
                    {
                        return true;
                    }
                    
                    

                }

            }

           return false;
        }

        void OnControllerColliderHit (ControllerColliderHit hit)
        {

            Ray ray = new Ray(transform.position + new Vector3(0, Controller.bounds.extents.y,0), hit.point- transform.position);
            RaycastHit rayHit;

             if(hit.collider.Raycast(ray, out rayHit, 10f))
             {
                 LastCollisionDistance = rayHit.point;
                 SlideNormal = rayHit.normal;
                 coll = rayHit.collider;
             }

        }

    }

}
