using UnityEngine;
using System.Collections;

public class droneController : MonoBehaviour
{

    //GameObjects and Rigidbody
    private GameObject leftChassis;
    private GameObject rightChassis;
    private Rigidbody drone;
    private AudioSource droneSound;
    private AudioSource crashSound;

    [Header("Values")]
    public float moveSpeed = 60;
    public float thrustSpeed = 80;
    public Rigidbody[] propellers;
    public bool takeOff = false;

    private int totalPropellers = 0;
    
    //Chassis
    private float moveChassis;

    //General Movement
    private string moveType;
    public float currentAltitude;

    //Thrust
    private float upForce;

    //Pitch
    //private float pitchInput = 0;
    private float pitchTilt = 0f;
    private float tiltVelocityForward;

    //Roll
    //private float rollInput = 0;
    private float rollTilt = 0;
    private float tiltVelocitySideways;

    //Yaw
    //private float yawInput = 0;
    private float wantedYRotation;
    private float yawRotation;
    private float yawSpeed = 1;
    private float rotationYVelocity;




    public float RollAngle { get; private set; }
    public float PitchAngle { get; private set; }

    //private void CalculateRollAndPitchAngles()
    //{
    //    // Calculate roll & pitch angles
    //    // Calculate the flat forward direction (with no y component).
    //    var flatForward = transform.forward;
    //    flatForward.y = 0;
    //    // If the flat forward vector is non-zero (which would only happen if the plane was pointing exactly straight upwards)
    //    if (flatForward.sqrMagnitude > 0)
    //    {
    //        flatForward.Normalize();
    //        // calculate current pitch angle
    //        var localFlatForward = transform.InverseTransformDirection(flatForward);
    //        PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
    //        // calculate current roll angle
    //        var flatRight = Vector3.Cross(Vector3.up, flatForward);
    //        var localFlatRight = transform.InverseTransformDirection(flatRight);
    //        RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
    //    }
    //}

    void Awake()
    {
        //Initialize everything
        GameObject tmp = GameObject.FindGameObjectWithTag("dronePrefab");
        crashSound = tmp.GetComponent<AudioSource>();
        tmp = GameObject.FindGameObjectWithTag("droneSound");
        droneSound = tmp.GetComponent<AudioSource>();
        upForce = 0f;
        moveType = "";
        drone = this.GetComponent<Rigidbody>();
        leftChassis = GameObject.FindGameObjectWithTag("leftChassis");
        rightChassis = GameObject.FindGameObjectWithTag("rightChassis");
    }

    /// Use this for initialization
    void Start()
    {
        totalPropellers = propellers.Length;
        //distToGround = drone.GetComponent<Collider>().bounds.extents.y;
    }

    ///  called once per frame
     void Update()
     {
        currentAltitude = Mathf.Round(transform.position.y - 4);
    }


    /// called once per physics calculation
    void FixedUpdate()
    {
        move();
    }



    public void move(float pitchInput = 0, float rollInput = 0, float yawInput = 0)
    {
        if (currentAltitude <= 0)
            takeOff = false;
        
        thrust();       //takeOff will be true at take-off

        if (takeOff)
        {
            pitch(pitchInput);
            roll(rollInput);
            yaw(yawInput);
        }

        clampVelocity(); //Defines maximum values for velocity
        flightMode();    //Handles the chassis control
        sound();         
        
        //Tilt depending on pitch, roll and yaw
        drone.rotation = Quaternion.Euler(new Vector3(pitchTilt, yawRotation, rollTilt));

        //Add an animation and a force for each propeller
        propellerController();
    }



    /// <summary>
    /// Calls the method propeller depending on the movetype for each propeller
    /// </summary>
    private void propellerController()
    {
        for (int i = 0; i < totalPropellers; i++)
        {
            if (moveType.Equals("thrust"))
                propeller(i, upForce, 0);

            if (moveType.Equals("forward"))
            {
                if (i == 0 || i == 1 || i == 6 || i == 7)
                    propeller(i, upForce, 40);
                else
                    propeller(i, upForce, -30);
            }
            if (moveType.Equals("backward"))
            {
                if (i == 0 || i == 1 || i == 6 || i == 7)
                    propeller(i, upForce, -30);
                else
                    propeller(i, upForce, 40);
            }
            if (moveType.Equals("left"))
            {
                if (i <= 3)
                    propeller(i, upForce, 40);
                else
                    propeller(i, upForce, -30);
            }
            if (moveType.Equals("right"))
            {
                if (i <= 3)
                    propeller(i, upForce, -30);
                else
                    propeller(i, upForce, 40);
            }

        }
    }




    /// <summary>
    /// Add Force and animate eath propeller
    /// </summary>
    /// <param name="index"></param>
    /// <param name="verticalForce"></param>
    /// <param name="propellerSpeed"></param>
    void propeller(int index, float verticalForce, float propellerSpeed)
    {
        float rotationSpeed = 0f;
        if (takeOff)
            rotationSpeed = drone.velocity.y + 40 + propellerSpeed;
        else
            rotationSpeed = 0;

        //two propellers build a counter-rotating pair to avoid the torque
        if (index % 2 == 0)
            propellers[index].transform.Rotate(0, rotationSpeed, 0);
        else
            propellers[index].transform.Rotate(0, -rotationSpeed, 0);

        propellers[index].AddRelativeForce(Vector3.up * (verticalForce / 8));
    }



    void sound()
    {
        //Creats a droneSound depending on the velocity
        droneSound.pitch = 0.8f + (drone.velocity.magnitude / 100);

        if (takeOff)
        {
            droneSound.mute = false;
            crashSound.mute = false;
        }
        else
        {
            droneSound.mute = true;
            crashSound.mute = true;
        }
    }



    /// <summary>
    /// Calculate the angular velocity w= 2pi / T
    /// T= 2pi* r / v
    /// </summary>
    /// <returns>the angular velocity in rad/s</returns>
    float getAngularVelocity()
    {
        float T = (2 * Mathf.PI)*10 / thrustSpeed;
        //convert (rad per sec) to (round per minute); multiply with 9.55
        return ( ( (Mathf.PI * 2) / T ) * 9.55f) ;
    }






    /// <summary>
    /// The Thrust moves the drone up and down
    /// Creates a vertical Force for each propeller
    /// </summary>
    void thrust()
    {
        //determines whether sound is played or not
        //checkSound();

        if (Input.GetAxis("LeftJoystickVertical") != 0 )
            moveType = "thrust";

        if (Input.GetKey(KeyCode.I) || Input.GetAxis("LeftJoystickVertical") > 0)
        {
            //upForce = getAngularVelocity();
            upForce = thrustSpeed;
            takeOff = true;
        }
        else if (Input.GetKey(KeyCode.K) || Input.GetAxis("LeftJoystickVertical") < 0)
            upForce = -thrustSpeed;

        else if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K)
            && (Input.GetAxis("LeftJoystickVertical") == 0))
            if (takeOff)
                upForce = 9.807f * drone.mass;
            else
                upForce = 0;
        
    }

    

    /// <summary>
    /// The pitch moves the drone forward and backward
    /// </summary>
    /// <param name="pitchInput"></param>
    void pitch(float pitchInput)
    {
        //Only if there is an input
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("RightJoystickVertical") != 0)
        {
 
            //Get the input of Keyboard or game controller
            if (Input.GetAxis("Vertical") != 0)
                pitchInput = Input.GetAxis("Vertical");
            else if(Input.GetAxis("RightJoystickVertical") != 0)
                pitchInput = Input.GetAxis("RightJoystickVertical");

            //Check the direction to animate the propeller
            if (Input.GetAxis("Vertical") > 0 || Input.GetAxis("RightJoystickVertical") > 0)
                moveType = "forward";
            if (Input.GetAxis("Vertical") < 0 || Input.GetAxis("RightJoystickVertical") < 0)
                moveType = "backward";
            
            drone.AddRelativeForce(Vector3.forward * pitchInput * moveSpeed);
            pitchTilt = Mathf.SmoothDamp(pitchTilt, pitchInput, ref tiltVelocityForward, 0.1f);
        }
        else
        {
            //Propeller rotating with same velocity again
            moveType = "thrust";

            //Stop tilting
            pitchTilt = Mathf.SmoothDamp(pitchTilt, 0, ref tiltVelocityForward, 0.1f);
        }
    }



    /// <summary>
    /// Roll moves the drone left/right sideways
    /// The Propellers on side rotate faster than the other
    /// </summary>
    /// <param name="rollInput"></param>
    void roll(float rollInput)
    {
        //Only if there is an input
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("RightJoystickHorizontal") != 0)
        {

            //Get the input of Keyboard or game controller
            if (Input.GetAxis("Horizontal") != 0)
                rollInput = Input.GetAxis("Horizontal");
            else if(Input.GetAxis("RightJoystickHorizontal") != 0)
                rollInput = Input.GetAxis("RightJoystickHorizontal");

            //Check the direction to animate the propeller
            if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("RightJoystickHorizontal") > 0)
                moveType = "right";
            if (Input.GetAxis("Horizontal") < 0 || Input.GetAxis("RightJoystickHorizontal") < 0)
                moveType = "left";

            drone.AddRelativeForce(Vector3.right * rollInput * moveSpeed);
            rollTilt = Mathf.SmoothDamp(rollTilt,  -1 * rollInput, ref tiltVelocitySideways, 0.1f);
        }
        else
        {
            //Stop tilting
            rollTilt = Mathf.SmoothDamp(rollTilt, 0, ref tiltVelocitySideways, 0.1f);
        }

    }



    /// <summary>
    /// Rotates the drone around the own axis
    /// </summary>
    /// <param name="yawInput"></param>
    void yaw(float yawInput)
    {
        if (takeOff)
        {
            if (Input.GetKey(KeyCode.J) || Input.GetAxis("LeftJoystickHorizontal") < 0)
                wantedYRotation -= yawSpeed;
            //drone.AddRelativeTorque(-1*Vector3.forward * rotationFactor, ForceMode.Force);

            //wantedYRotation -= rotateAmountByKeys;

            if (Input.GetKey(KeyCode.L) || Input.GetAxis("LeftJoystickHorizontal") > 0)
                wantedYRotation += yawSpeed;
            //drone.AddRelativeTorque(Vector3.forward * rotationFactor, ForceMode.Force);
            
            //Create delay to simulate animation
            yawRotation = Mathf.SmoothDamp(yawRotation, wantedYRotation, ref rotationYVelocity, 0.4f);
        }
    }


    /// <summary>
    /// Clamp the drone velocity smoothly
    /// </summary>
    private Vector3 velocityToSmoothDamp;
    void clampVelocity()
    {
        //Move forward and sideways input
        if ((Input.GetAxis("Vertical") != 0 && Input.GetAxis("Horizontal") != 0) ||
             (Input.GetAxis("RightJoystickHorizontal") != 0 && Input.GetAxis("RightJoystickVertical") != 0))
            drone.velocity = Vector3.ClampMagnitude(drone.velocity, Mathf.Lerp(drone.velocity.magnitude, 20.0f, Time.deltaTime * 5f));
        
        //Move forward only
        if ((Input.GetAxis("Vertical") != 0 && Input.GetAxis("Horizontal") == 0) ||
            (Input.GetAxis("RightJoystickVertical") != 0 && Input.GetAxis("RightJoystickHorizontal") == 0))
            drone.velocity = Vector3.ClampMagnitude(drone.velocity, Mathf.Lerp(drone.velocity.magnitude, 20.0f, Time.deltaTime * 5f));

        //Move sideways only
        if ((Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") != 0) ||
            (Input.GetAxis("RightJoystickVertical") == 0 && Input.GetAxis("RightJoystickHorizontal") != 0))
            drone.velocity = Vector3.ClampMagnitude(drone.velocity, Mathf.Lerp(drone.velocity.magnitude, 20.0f, Time.deltaTime * 5f));

        //No movement
        //Smoothly stop of current movement 
        if ((Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0) ||
            (Input.GetAxis("RightJoystickVertical") == 0 && Input.GetAxis("RightJoystickHorizontal") == 0))
            drone.velocity = Vector3.SmoothDamp(drone.velocity, Vector3.zero, ref velocityToSmoothDamp, 0.95f);
        
    }



    /// <summary>
    /// Retracts the chassis 
    /// </summary>
    void flightMode()
    {
        float currentLeftAngle = Mathf.Round(leftChassis.transform.localRotation.eulerAngles.z);
        //float currentRightAngle = Mathf.Round(rightChassis.transform.localRotation.eulerAngles.z);

        if (Input.GetKey(KeyCode.Y) || Input.GetButton("L1"))
        {
            //Debug.Log("Y: left: " + currentLeftAngle + "; Right: " + currentLeftAngle + "; " + (270 <= currentLeftAngle && currentLeftAngle <= 360));
            if ((270 < currentLeftAngle && currentLeftAngle <= 359) || (currentLeftAngle == 0))
            {
                leftChassis.GetComponent<AudioSource>().enabled = true;
                moveChassis = 40 * Time.deltaTime;
                leftChassis.transform.Rotate(0, 0, -moveChassis);
                rightChassis.transform.Rotate(0, 0, moveChassis);
            }
        }else

        if (Input.GetKey(KeyCode.X) || Input.GetButton("R1"))
        {
            //Debug.Log("X: left: " + currentLeftAngle + "; Right: " + currentRightAngle + "; " + (270 <= currentLeftAngle && currentLeftAngle <= 360));
            if ((270 <= currentLeftAngle && currentLeftAngle <= 359))
            {
                leftChassis.GetComponent<AudioSource>().enabled = true;
                moveChassis = 40 * Time.deltaTime;
                leftChassis.transform.Rotate(0, 0, moveChassis);
                rightChassis.transform.Rotate(0, 0, -moveChassis);
            }
        }
        else
            leftChassis.GetComponent<AudioSource>().enabled = false;

    }




    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        if (collision.relativeVelocity.magnitude > 2)
            crashSound.Play();

    }

}
