using UnityEngine;

namespace Assets.StarSystem.Physics
{
    public class PlanetAttractor : MonoBehaviour
    {
        public float mass = 100000f;

        public void Attract(Transform body)
        {
            Vector3 gravityUp = (body.position - transform.position);
            float distance = gravityUp.magnitude;
            gravityUp = gravityUp.normalized;
            Vector3 bodyUp = body.up;

            var rigidbody = body.GetComponent<Rigidbody>();
            rigidbody.AddForce(-gravityUp * (mass * rigidbody.mass / (Mathf.Pow(distance, 2))));

            Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * body.rotation;
            body.rotation = targetRotation;// Quaternion.Slerp(body.rotation, targetRotation, 50 * Time.deltaTime);
        }
    }
}
