using Assets.StarSystem.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Utils
{
    [RequireComponent(typeof(Camera))]
    public class LagCamera : MonoBehaviour
    {
        public SystemController controller;
        public GameObject spaceShip;
        public float maxdistance = 10f;

        [Tooltip("Speed at which the camera rotates. (Camera uses Slerp for rotation.)")]
        public float rotateSpeed = 90.0f;

        private Transform target;
        private Quaternion rot;
        private Vector3 position;
        private Rigidbody spaceShipRbody;

        private void Start()
        {
            target = transform.parent;
            rot = transform.rotation;
            position = transform.position;
            spaceShipRbody = spaceShip.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (target != null)
            {
                rot = Quaternion.Slerp(rot, target.rotation, rotateSpeed * Time.deltaTime);
                transform.rotation = rot;

                float speedFactor = Mathf.Clamp(spaceShipRbody.velocity.magnitude / 200f, 0, 1);

                Vector3 localTarget = new Vector3(0, -0.6f, 2) + new Vector3(0, 0, -1) * speedFactor * maxdistance;
                Vector3 worldTarget = target.TransformPoint(localTarget);

                position = Vector3.Slerp(position, worldTarget, Time.deltaTime * 40f);

                Vector3 local = target.InverseTransformPoint(position);
                local.z = 2;

                Camera.main.fieldOfView = 60 + 30 * speedFactor;

                transform.localPosition = local;
            }
        }
    }
}
