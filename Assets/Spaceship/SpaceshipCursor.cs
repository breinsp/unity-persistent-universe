using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceshipCursor : MonoBehaviour
{

    public Sprite crosshair;
    public GameObject imagePrefab;

    private GameObject crosshair_go;

    // Use this for initialization
    void Start()
    {
        crosshair_go = Instantiate(imagePrefab);
        crosshair_go.transform.SetParent(transform, false);
        crosshair_go.GetComponent<Image>().sprite = crosshair;
        var crosshair_rect = crosshair_go.GetComponent<RectTransform>();
        crosshair_rect.localPosition = Vector3.zero;
        crosshair_rect.localScale = Vector3.one * .16f;
        crosshair_go.name = "crosshair";
    }
}
