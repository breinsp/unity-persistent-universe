using Assets.Utils;
using System;
using UnityEngine;

namespace Assets.StarSystem.Generation
{
    public class CelestialBody
    {
        public int seed;
        public int radius;

        public double mass;
        public double density;

        public double orbitDistance;

        public double oribtalVelocity;
        public double orbitalOffset;
        public double rotationSpeed;

        public GameObject gameObject;
        public GameObject originalGameObject;
        public SystemController controller;
        public Vector3 position;
        public Vector3 rotationAxis;
        public Quaternion rotation;
        //position when another planet is zoomed
        public Vector3 relativePosition;
        public CelestialType type;
        public float atmosphereRadiusMultiplier = 1.07f;
        public CelestialBody host;

        public bool zoomed;

        public bool InsideTransition
        {
            get
            {
                var threshold = (zoomed ? controller.planetScaleMultiplier : 1) * radius * controller.LODdistances[controller.LODdistances.Length - 4];
                var pos = zoomed ? Vector3.zero : position;
                return (controller.playerPosition - pos).magnitude > threshold;
            }
        }

        public bool FarAway
        {
            get
            {
                var threshold = (zoomed ? controller.planetScaleMultiplier : 1) * radius * controller.LODdistances[controller.LODdistances.Length - 1];
                var pos = zoomed ? Vector3.zero : position;
                return (controller.playerPosition - pos).magnitude > threshold;
            }
        }

        public float MaximumVisibleDistance
        {
            get
            {
                float rad = radius * (controller.zoomed == null ? 1 : controller.planetScaleMultiplier);
                float distance = (controller.playerPosition - (controller.zoomed == null ? position : Vector3.zero)).magnitude;
                float max = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(distance, 2) - Mathf.Pow(rad, 2)));

                return max;
            }
        }

        public CelestialBody(SystemController controller, GameObject gameObject, int seed, CelestialType type, CelestialBody host = null)
        {
            if (host != null)
            {
                type = new CelestialType.Moon(type);
            }
            gameObject.name = type.ToString();
            this.controller = controller;
            this.seed = seed;
            this.type = type;
            this.gameObject = gameObject;
            this.host = host;
            System.Random random = new System.Random(seed);
            radius = (int)Utility.GetRandomBetween(random, type.minRadius, type.maxRadius);
            density = type.density;
            orbitDistance = Utility.GetRandomBetween(random, type.minDistance, type.maxDistance);

            mass = (4.0 / 3.0) * Math.PI * Math.Pow(radius, 3.0) * density;
            oribtalVelocity = Utility.GetRandomBetween(random, 0.5, 3) / 360.0 * (type is CelestialType.Moon ? 100 : 1);
            orbitalOffset = Utility.GetRandomBetween(random, 0, 360);
            rotationAxis = Utility.RandomVector(random);
            rotationSpeed = Utility.GetRandomBetween(random, 50, 100);
            UpdatePositionInSystem();

        }

        DateTime centuryBegin = new DateTime(2001, 1, 1);

        public void UpdatePositionInSystem()
        {
            DateTime currentDate = DateTime.Now;
            long elapsedTicks = currentDate.Ticks - centuryBegin.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

            double orbitAngle = (orbitalOffset + elapsedSpan.TotalHours * oribtalVelocity);
            Vector3 orbitAxis = host == null ? Vector3.up : host.rotationAxis;
            Vector3 pivot = host == null ? Vector3.zero : host.position;

            float x = (float)(Math.Sin(orbitAngle) * orbitDistance);
            float z = (float)(Math.Cos(orbitAngle) * orbitDistance);

            Vector3 orbitalPosition = new Vector3(x, 0, z);
            Quaternion orbitalRotation = Quaternion.FromToRotation(Vector3.up, orbitAxis);
            Vector3 rotated = orbitalRotation * orbitalPosition + pivot;

            position = rotated;
            float rotationAngle = (float)(rotationSpeed * elapsedSpan.TotalMinutes) % 360f;
            rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis) * Quaternion.AngleAxis(rotationAngle, Vector3.up);

            if (controller.zoomed == null)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
            }
        }

        public override string ToString()
        {
            return type.ToString();
        }
    }
}
