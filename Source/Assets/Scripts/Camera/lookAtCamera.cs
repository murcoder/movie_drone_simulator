using UnityEngine;
using System.Collections;

public class lookAtCamera : MonoBehaviour {

    public GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Comes after update()
    void LateUpdate()
    {
        transform.LookAt(target.transform);
    }
}
