using UnityEngine;

namespace Assets.StarSystem.Physics
{
    public class AttractableBody : MonoBehaviour
    {/*
        public SystemController controller;
        private Transform myTransform;

        private void Start()
        {
            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.useGravity = false;
            myTransform = transform;
        }

        private void Update()
        {
            
            if (controller.celestials != null)
            {
                var closest_planet = controller.celestials.OrderBy(p => (p.position - myTransform.position).magnitude).FirstOrDefault();
                if (closest_planet != null)
                    closest_planet.gameObject.GetComponent<PlanetAttractor>().Attract(myTransform);
            }
            
    }*/
    }
}
