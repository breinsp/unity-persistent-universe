using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
    public class Vector3EqualityComparer : EqualityComparer<Vector3>
    {
        public override bool Equals(Vector3 x, Vector3 y)
        {
            return RoundVector(x).Equals(RoundVector(y));
        }

        public override int GetHashCode(Vector3 obj)
        {
            return RoundVector(obj).GetHashCode();
        }

        private Vector3 RoundVector(Vector3 P)
        {
            int decimals = 4;
            float factor = Mathf.Pow(10, decimals);

            return new Vector3(
                Mathf.Round(P.x * factor) / factor,
                Mathf.Round(P.y * factor) / factor,
                Mathf.Round(P.z * factor) / factor
            );
        }
    }
}
