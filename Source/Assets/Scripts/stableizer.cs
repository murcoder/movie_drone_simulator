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
    Transform mTransform;

    //Update 02-01-2017: Adopted to Octodrone
    public Rigidbody[] propGuards;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        mTransform = GetComponent<Transform>();

        //Get the four corners of the object
        frontLeft = new Vector3(-mTransform.localScale.x, 0, mTransform.localScale.x);
        frontRight = new Vector3(mTransform.localScale.x, 0, mTransform.localScale.x);
        rearLeft = new Vector3(-mTransform.localScale.x, 0, -mTransform.localScale.x);
        rearRight = new Vector3(mTransform.localScale.x, 0, -mTransform.localScale.x);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
        if (body.GetComponent<droneController>().takenOff)
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

            Vector3 orientation = mTransform.localRotation.eulerAngles;
            orientation.y = 0;
            FixRanges(ref orientation);

            //Get the angular Velocity of the drone in local space (rad/s)
            Vector3 localangularvelocity = mTransform.InverseTransformDirection(body.angularVelocity);

            float velY = body.velocity.y;

            //Vorwärtsneigung (Pitch)
            float desiredForward = forward * MAX_TILT - (orientation.x + localangularvelocity.x * 15);

            //Seitwärtsneigung (Roll)
            float desiredRight = -right * MAX_TILT - (orientation.z + localangularvelocity.z * 15);

            //Rotation (Yaw)
            float desiredSpin = spin - localangularvelocity.y;

            ApplyForces(desiredForward / MAX_TILT, desiredRight / MAX_TILT, up - velY, desiredSpin);

        }
        else
            body.GetComponent<droneController>().upForce = (9.807f * body.mass + 0.8f);

    }

    void ApplyForces(float forward, float right, float up, float spin)
    {
        //Make sure, that upForce is not higher then maximum
        float totalY = Mathf.Min((up * 100) + (9.807f * body.mass + 0.8f) , MAX_FORCE);

        if (totalY < 0) totalY = 0;
        
        //ORIGINAL
        //distribute according to forward/right (which are indices based on max tilt)
        //Adding also random noise
        //front left
        body.AddForceAtPosition(Random.Range(0.95f,1) * mTransform.up * ((totalY * .25f) - (forward * STEER_FORCE) - (right * STEER_FORCE)),    mTransform.position + mTransform.TransformDirection(frontLeft));

        //front right
        body.AddForceAtPosition(Random.Range(0.95f, 1) * mTransform.up * (totalY * .25f - forward * STEER_FORCE + right * STEER_FORCE),     mTransform.position + mTransform.TransformDirection(frontRight));

        //rear left
        body.AddForceAtPosition(Random.Range(0.95f, 1) * mTransform.up * (totalY * .25f + forward * STEER_FORCE - right * STEER_FORCE),    mTransform.position + mTransform.TransformDirection(rearLeft));

        //rear right
        body.AddForceAtPosition(Random.Range(0.95f, 1) * mTransform.up * (totalY * .25f + forward * STEER_FORCE + right * STEER_FORCE),     mTransform.position + mTransform.TransformDirection(rearRight));

        //Make sure, that spin is not higher then maximum
        spin = Mathf.Min(MAX_SPIN, spin);

        //Rear
        body.AddForceAtPosition(-mTransform.right * spin, mTransform.position - mTransform.forward);
        //Front
        body.AddForceAtPosition(mTransform.right * spin, mTransform.position + mTransform.forward);

        //UPDATE 02-01-2017: adjusted for Octodrone
        //for (int i = 0; i < propGuards.Length; i++)
        //{
        //    //Check if right or left
        //    if (i <= 3)
        //    {

        //        //Check if front or rear
        //        if (i == 0 || i == 1)
        //            propGuards[i].AddForceAtPosition(mTransform.up * (totalY * .125f + forward * STEER_FORCE + right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //        else 
        //            propGuards[i].AddForceAtPosition(mTransform.up * (totalY * .125f - forward * STEER_FORCE + right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //    }
        //    else
        //    {
        //        if (i == 4 || i == 5)
        //            propGuards[i].AddForceAtPosition(mTransform.up * (totalY * .125f - forward * STEER_FORCE - right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //        else 
        //            propGuards[i].AddForceAtPosition(mTransform.up * (totalY * .125f + forward * STEER_FORCE - right * STEER_FORCE), propGuards[i].transform.TransformDirection(propGuards[i].position));
        //    }

        //}


        //spin = Mathf.Min(MAX_SPIN, spin);
        //for (int i = 0; i < propGuards.Length; i++)
        //{

        //    if (i == 0 || i == 1 || i == 6 || i == 7) //Back
        //        propGuards[i].AddForceAtPosition(-mTransform.right * spin, propGuards[i].transform.position - propGuards[i].transform.forward);
        //    else       //Front
        //        propGuards[i].AddForceAtPosition(mTransform.right * spin, propGuards[i].transform.position + propGuards[i].transform.forward);
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