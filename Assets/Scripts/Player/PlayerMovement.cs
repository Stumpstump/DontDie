﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {

        [Tooltip("In Percent")]
        public float SprintingSpeedModifier;

        public float MaxCameraXAngle;
        public float JumpHeight;
        public float WindDownDuration;
        public float Gravity;
        public float JumpDuration;
        public float WindUpDuration;

        public float ForwardSpeed;
        public float BackwardSpeed;
        public float StrafeSpeed;

        [SerializeField]
        private float SpeedFactor = 100;

        private Camera FirstPersonCamera;
        private CharacterController Controller;
        private PlayerMovementStatus MovementStatus;
        private PlayerMovementStatus LastMovementStatus;
        private Vector3 DirectionalMovementInputs = new Vector3();
        private Vector3 LastDirectionalMovementInputs = new Vector3();
        private float Speed = 0;
   
        private bool isWindingDown = false;
        private bool isWindingUp = false;
        private bool isJumping = false;
        bool stopWindingUp = false;
        bool stopWindingDown = false;

        Coroutine WindUpCouroutine;
        Coroutine WindDownCouroutine;

        // Start is called before the first frame update
        void Awake()
        {
            Controller = GetComponent<CharacterController>();
            FirstPersonCamera = Camera.main;
        }


        // Update is called once per frame
        void Update()
        {
            DirectionalMovementInputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            DirectionalMovementInputs.Normalize();
            UpdateMovementStatus();
            UpdateSpeed();
            Rotate();
            
            switch(MovementStatus)
            {
                case PlayerMovementStatus.Walking:              
                    {
                        Move();                
                        break;
                    }

                case PlayerMovementStatus.Idling:
                    {
                        LastDirectionalMovementInputs = new Vector3(0, 0, 0);
                        Controller.Move(new Vector3(0, Gravity, 0) * Time.deltaTime);
                        break;
                    }
            }            
        }

        void Move()
        {
            if (WindDownCouroutine != null)
                DirectionalMovementInputs = LastDirectionalMovementInputs;

            LastDirectionalMovementInputs = DirectionalMovementInputs;

            Vector3 Movement = transform.TransformDirection(DirectionalMovementInputs) * Time.deltaTime * Speed * SpeedFactor / 100;
            Movement.y = Gravity * Time.deltaTime;

            Controller.Move(Movement);                            
        }

        void Rotate()
        {
            Vector3 CharacterRotation = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
            Vector3 CameraRotation = new Vector3(0, FirstPersonCamera.transform.eulerAngles.y, FirstPersonCamera.transform.eulerAngles.z);

            CharacterRotation.y = transform.eulerAngles.y - Input.GetAxisRaw("Mouse X") * -1;
            CameraRotation.x = FirstPersonCamera.transform.eulerAngles.x - Input.GetAxisRaw("Mouse Y");

            if (FirstPersonCamera.transform.eulerAngles.x > MaxCameraXAngle) CameraRotation.x -= 360;

            CameraRotation.x = Mathf.Clamp(CameraRotation.x, -MaxCameraXAngle, MaxCameraXAngle);

            FirstPersonCamera.transform.eulerAngles = CameraRotation;
            transform.eulerAngles = CharacterRotation;
        }

        void Jump()
        {
            if(Controller.isGrounded)
            {
                StartCoroutine(JumpEvent());
                isJumping = true;
            }
        }

        IEnumerator JumpEvent()
        {
            Vector3 MovementTillNow = new Vector3();
            Vector3 Direction = transform.TransformDirection(LastDirectionalMovementInputs) * Speed * SpeedFactor / 100;
            Direction.y = 0f;

            float JumpTime = 0f;
            do
            {
                JumpTime += Time.deltaTime;

                Controller.Move(Math.MathParabola.Parabola(Vector3.zero, Direction, JumpHeight, JumpTime, JumpDuration) - MovementTillNow);
                MovementTillNow = Math.MathParabola.Parabola(Vector3.zero, Direction, JumpHeight, JumpTime, JumpDuration);                
                yield return null;

            } while (!Controller.isGrounded && Controller.collisionFlags != CollisionFlags.Above);

            isJumping = false;
        }

        void UpdateSpeed()
        {
            if (isJumping)
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
            if (isJumping) return;

            if (Input.GetKeyDown(KeyCode.Space))
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

        public void ChangeSpeedFactor(int newFactor)
        {
            SpeedFactor = newFactor;            
        }

        IEnumerator WindUp()
        {
           
            float WindUpCounter = 0f;
            isWindingUp = true;
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
            isWindingUp = false;
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
            isWindingDown = false;
            WindDownCouroutine = null;
        }

    }

}