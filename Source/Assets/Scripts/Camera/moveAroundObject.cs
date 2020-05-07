using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveAroundObject : MonoBehaviour {

    [Header("Values")]
    public Transform target;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

            transform.LookAt(target);
            transform.Translate(Vector3.right * Time.deltaTime);
        }
    }

