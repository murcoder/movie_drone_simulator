using UnityEngine;
using System.Collections;

public class MassCenter : MonoBehaviour {

    public Transform massCenter;

	// Use this for initialization
	void Start () {
        Rigidbody rigid = this.GetComponent<Rigidbody>();
        //Set local position as middle of mass
        //Transform test = 
        rigid.centerOfMass = massCenter.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
