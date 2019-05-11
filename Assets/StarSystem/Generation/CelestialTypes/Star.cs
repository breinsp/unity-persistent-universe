using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes
{
    public class Star : CelestialBody
    {
        public Star(SystemController controller, int seed, GameObject gameObject) : base(controller, gameObject, seed, new CelestialType.Star())
        {
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            filter.mesh = Octahedron.Create(5, radius);
            Material mat = new Material(controller.materialController.starMaterial);
            CelestialType starType = new CelestialType.Star();
            mat.SetInt("_Temp", (int)Utility.GetRandomBetween(new System.Random(seed), starType.minTemperature, starType.maxTemperature));
            renderer.material = mat;
            renderer.receiveShadows = false;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            gameObject.transform.localScale = Vector3.one;

            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 2000000;
            light.bounceIntensity = 0;
            light.renderMode = LightRenderMode.ForcePixel;
            light.intensity = 1;
            light.shadows = LightShadows.Soft;
            light.shadowNearPlane = .1f;
        }
    }
}
