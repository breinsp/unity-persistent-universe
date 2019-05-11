using Assets.StarSystem.Generation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.StarSystem.UI
{

    public class PlanetHud : MonoBehaviour
    {

        public SystemController controller;

        public List<KeyValuePair<CelestialBody, GameObject>> planetHuds;

        public void GeneratePlanetHuds()
        {
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            planetHuds = new List<KeyValuePair<CelestialBody, GameObject>>();
            foreach (var c in controller.celestials)
            {
                GameObject textGo = new GameObject("HUD: " + c.ToString());
                textGo.transform.parent = transform;
                Text text = textGo.AddComponent<Text>();
                text.font = font;
                text.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
                planetHuds.Add(new KeyValuePair<CelestialBody, GameObject>(c, textGo));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (planetHuds != null)
            {
                foreach (var pair in planetHuds)
                {
                    Vector3 player = controller.playerPosition;
                    Vector3 planet = pair.Key.position;
                    float distance = (planet - player).magnitude;

                    string text = pair.Key.ToString() + " [" + Mathf.Round(distance*controller.planetScaleMultiplier/1000) + "k units]";

                    Vector3 screenPos = Camera.main.WorldToScreenPoint(pair.Key.position);
                    if (screenPos.z < 0 || controller.zoomed != null) text = "";
                    screenPos.z = 0;
                    pair.Value.transform.position = screenPos;
                    pair.Value.GetComponent<Text>().text = text;
                }
            }
        }
    }
}