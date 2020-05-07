using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demoHuman : MonoBehaviour {


    private Animator anim;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        anim.SetBool("move", true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
