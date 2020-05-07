using UnityEngine;
using System.Collections;

public class follow : MonoBehaviour {

    [Header("Values")]
    public GameObject target;
    public float damping = 5;
    private Vector3 offset;
     
    // Use this for initialization
    void Start () {
        offset = target.transform.position - transform.position;
    }
	 
	// Update is called once per frame
	void Update () {
	 
	}
     
    void LateUpdate()
    {
        //Get the angles of camera and player 
        //eulerAngles  global rotation in degree
        float currentAngle = transform.eulerAngles.y;
        float desiredAngle = target.transform.eulerAngles.y;
       
        //Lerp makes an smooth movement using linear interpolation
        float angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);
      
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        transform.position = target.transform.position - (rotation * offset);

        //Focus Z-Axis at the player
        transform.LookAt(target.transform);
    }
}
