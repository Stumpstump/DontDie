using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Player
{
    [Serializable]
    public class PlayerCamera
    {
        [SerializeField] private Camera FirstPersonCamera;
        [SerializeField] private float Sensitivity;
        [SerializeField] private float MaxXAngle;
        [SerializeField] private float MinXAngle;

        private Transform Owner;

        public void Initialize(Transform newOwner)
        {
            Owner = newOwner;    
        }

        public void Rotate()
        {
            Vector3 CharacterRotation = new Vector3(Owner.eulerAngles.x, 0, Owner.eulerAngles.z);
            Vector3 CameraRotation = new Vector3(0, FirstPersonCamera.transform.eulerAngles.y, FirstPersonCamera.transform.eulerAngles.z);

            CharacterRotation.y = Owner.eulerAngles.y - Input.GetAxisRaw("Mouse X") * -1 * Sensitivity/ 100;
            CameraRotation.x = FirstPersonCamera.transform.eulerAngles.x - Input.GetAxisRaw("Mouse Y") * Sensitivity / 100;

            if (FirstPersonCamera.transform.eulerAngles.x > MaxXAngle) CameraRotation.x -= 360;

            CameraRotation.x = Mathf.Clamp(CameraRotation.x, MinXAngle, MaxXAngle);

            FirstPersonCamera.transform.eulerAngles = CameraRotation;
            Owner.eulerAngles = CharacterRotation;
        }
    }
}
