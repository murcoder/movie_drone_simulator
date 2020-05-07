using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class stableizer : MonoBehaviour
{

    const float MAX_FORCE = 50;
    const float MAX_TILT = 50;

    //The Velocity of tilting
    const float STEER_FORCE = .5f;
    const float MAX_SPIN = .2f;

    //Four corners of the object
    Vector3 frontLeft, frontRight, rearLeft, rearRight;

    Rigidbody body;
    Transform droneTransform;

    //Update 02-01-2017: Adopted to Octodrone
    public Rigidbody[] propGuards;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        droneTransform = GetComponent<Transform>();

        //Get the four corners of the object inlcuding the scalefactor
        frontLeft = new Vector3(-droneTransform.localScale.x, 0, droneTransform.localScale.x);
        frontRight = new Vector3(droneTransform.localScale.x, 0, droneTransform.localScale.x);
        rearLeft = new Vector3(-droneTransform.localScale.x, 0, -droneTransform.localScale.x);
        rearRight = new Vector3(droneTransform.localScale.x, 0, -droneTransform.localScale.x);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
        if (body.GetComponent<droneController>().takeOff)
        {
            float forward = 0;
            float right = 0;
            float up = 0;
            float spin = 0;


            //KEYBOARD 
            if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 || Input.GetKey(KeyCode.I)
                || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L))
            {
                forward = Input.GetAxis("Vertical");
                right = Input.GetAxis("Horizontal");

                if (Input.GetKey(KeyCode.I))
                    up = 100 * 20;
                if (Input.GetKey(KeyCode.K))
                    up = -100 * 20;
                if (Input.GetKey(KeyCode.J))
                    spin = -3.5f * 20;
                if (Input.GetKey(KeyCode.L))
                    spin = 3.5f * 20;

                //up = Input.GetAxis("LeftJoystickVertical") * 20;
                //spin = Input.GetAxis("LeftJoystickHorizontal") * 3.5f;
                //Debug.Log("Keyboard----up: " + up + "; spin: " + spin);
            }

            //GAMEPAD
            else if (Input.GetAxis("RightJoystickVertical") != 0 || Input.GetAxis("RightJoystickHorizontal") != 0 ||
                Input.GetAxis("LeftJoystickVertical") != 0 || Input.GetAxis("LeftJoystickHorizontal") != 0)
            {
                forward = Input.GetAxis("RightJoystickVertical");
                right = Input.GetAxis("RightJoystickHorizontal");
                up = Input.GetAxis("LeftJoystickVertical") * 20;
                spin = Input.GetAxis("LeftJoystickHorizontal") * 3.5f;
                //Debug.Log("up: " + up +"; spin: " + spin);

            }

            Vector3 rotateVec = droneTransform.localRotation.eulerAngles;
            rotateVec.y = 0;
            FixRanges(ref rotateVec);

            //Get the angular Velocity of the drone in local space (rad/s)
            Vector3 localangularvelocity = droneTransform.InverseTransformDirection(body.angularVelocity);

            float velY = body.velocity.y;

            //Vorwärtsneigung (Pitch)
            float desiredForward = forward * MAX_TILT - (rotateVec.x + localangularvelocity.x * 20);

            //Seitwärtsneigung (Roll)
            float desiredRight = -right * MAX_TILT - (rotateVec.z + localangularvelocity.z * 20);

            //Rotation (Yaw)
            float desiredSpin = spin - localangularvelocity.y;

            ApplyForces(desiredForward / MAX_TILT, desiredRight / MAX_TILT, up - velY, desiredSpin);

        }
        //else
            //body.GetComponent<droneController>().upForce = (9.807f * body.mass + 0.8f);

    }


    /// <summary>
    /// Compensate the desire to tilt
    /// </summary>
    /// <param name="forward">The desired forward-angle</param>
    /// <param name="right">The desired sideways-angle</param>
    /// <param name="up">The difference between current y-speed and y-input</param>
    /// <param name="spin">The desired Spin</param>
    void ApplyForces(float forward, float right, float up, float spin)
    {
        //Make sure upForce will not be higher then the maximum value
        float totalY = Mathf.Min((up * 100) , MAX_FORCE);
        if (totalY < 0) totalY = 0;

        //ORIGINAL
        //distribute according to forward/right and adding random noise
        //front left
        body.AddForceAtPosition(Random.Range(.95f,1) * droneTransform.up * ((totalY * .25f) - (forward * STEER_FORCE) - (right * STEER_FORCE)),    
            droneTransform.position + droneTransform.TransformDirection(frontLeft));
        //front right
        body.AddForceAtPosition(Random.Range(.95f, 1) * droneTransform.up * ((totalY * .25f) - (forward * STEER_FORCE) + (right * STEER_FORCE)),     
            droneTransform.position + droneTransform.TransformDirection(frontRight));
        //rear left
        body.AddForceAtPosition(Random.Range(.95f, 1) * droneTransform.up * ((totalY * .25f) + (forward * STEER_FORCE) - (right * STEER_FORCE)),    
            droneTransform.position + droneTransform.TransformDirection(rearLeft));
        //rear right
        body.AddForceAtPosition(Random.Range(.95f, 1) * droneTransform.up * ((totalY * .25f) + (forward * STEER_FORCE) + (right * STEER_FORCE)),     
            droneTransform.position + droneTransform.TransformDirection(rearRight));


        //Make sure, that spin is not higher then maximum
        spin = Mathf.Min(MAX_SPIN, spin);

        //Rear
        body.AddForceAtPosition(-droneTransform.right * spin, droneTransform.position - droneTransform.forward);
        //Front
        body.AddForceAtPosition(droneTransform.right * spin, droneTransform.position + droneTransform.forward);


        //TODO
        //UPDATE 02-01-2017: adjusted for Octocopter
        //for (int i = 0; i < propGuards.Length; i++)
        //{
        //    //Check if right or left
        //    if (i <= 3)
        //    {

        //        //Check if front or rear
        //        if (i == 0 || i == 1)
        //            propGuards[i].AddForceAtPosition(droneTransform.up * (totalY * .125f + forward * STEER_FORCE + right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //        else 
        //            propGuards[i].AddForceAtPosition(droneTransform.up * (totalY * .125f - forward * STEER_FORCE + right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //    }
        //    else
        //    {
        //        if (i == 4 || i == 5)
        //            propGuards[i].AddForceAtPosition(droneTransform.up * (totalY * .125f - forward * STEER_FORCE - right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //        else 
        //            propGuards[i].AddForceAtPosition(droneTransform.up * (totalY * .125f + forward * STEER_FORCE - right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //    }

        //}


        //spin = Mathf.Min(MAX_SPIN, spin);
        //for (int i = 0; i < propGuards.Length; i++)
        //{

        //    if (i == 0 || i == 1 || i == 6 || i == 7) //Back
        //        propGuards[i].AddForceAtPosition(-droneTransform.right * spin, propGuards[i].transform.position - propGuards[i].transform.forward);
        //    else       //Front
        //        propGuards[i].AddForceAtPosition(droneTransform.right * spin, propGuards[i].transform.position + propGuards[i].transform.forward);
        //}
        

    }

    void FixRanges(ref Vector3 euler)
    {
        if (euler.x < -180)
            euler.x += 360;
        else if (euler.x > 180)
            euler.x -= 360;

        if (euler.y < -180)
            euler.y += 360;
        else if (euler.y > 180)
            euler.y -= 360;

        if (euler.z < -180)
            euler.z += 360;
        else if (euler.z > 180)
            euler.z -= 360;
    }
}