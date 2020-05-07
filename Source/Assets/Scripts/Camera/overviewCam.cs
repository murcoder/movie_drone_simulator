using UnityEngine;
using System.Collections;

public class overviewCam : MonoBehaviour {

    [Header("Values")]
    public Transform target;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        transform.LookAt(target);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (GetComponent<MouseLook>().enabled == true)
                GetComponent<MouseLook>().enabled = false;
            else
                GetComponent<MouseLook>().enabled = true;

        }


    }
}
