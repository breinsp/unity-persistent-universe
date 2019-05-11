using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyCamera : MonoBehaviour {

    public float speed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float strafe = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        transform.position += new Vector3(strafe, 0, forward) * speed;
	}
}
