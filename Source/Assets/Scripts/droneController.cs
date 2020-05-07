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
    public float velocity = 5f;
    public float moveVelocity = 60;
    public float upForce;
    public Rigidbody[] propellers;
    public bool takenOff = false;

    private int totalPropellers = 0;
    
    //Chassis
    private float moveChassis;

    //General Movement
    private string moveType;
    public float currentAltitude;

    //Thrust
    private float thrustInput;
    private float thrustCoefficent;

    //Pitch
    //private float pitchInput = 0;
    private float currentForwardTilt = 0f;
    private float tiltVelocityForward;

    //Roll
    //private float rollInput = 0;
    private float currentSidewayTilt = 0;
    private float tiltVelocitySideways;

    //Yaw
    //private float yawInput = 0;
    private float wantedYRotation;
    private float currentYRotation;
    private float yawSpeed = 1;
    private float rotationYVelocity;




    public float RollAngle { get; private set; }
    public float PitchAngle { get; private set; }

    private void CalculateRollAndPitchAngles()
    {
        // Calculate roll & pitch angles
        // Calculate the flat forward direction (with no y component).
        var flatForward = transform.forward;
        flatForward.y = 0;
        // If the flat forward vector is non-zero (which would only happen if the plane was pointing exactly straight upwards)
        if (flatForward.sqrMagnitude > 0)
        {
            flatForward.Normalize();
            // calculate current pitch angle
            var localFlatForward = transform.InverseTransformDirection(flatForward);
            PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            // calculate current roll angle
            var flatRight = Vector3.Cross(Vector3.up, flatForward);
            var localFlatRight = transform.InverseTransformDirection(flatRight);
            RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }
    }

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

    /// Update is called once per frame
     void Update()
     {
        currentAltitude = Mathf.Round(transform.position.y - 4);
    }


    /// fixed update is called in time with the physics system update
    void FixedUpdate()
    {
        move();
    }



    public void move(float pitchInput = 0, float rollInput = 0, float yawInput = 0, float thrustInput = 0)
    {
        if (currentAltitude <= 0)
        {
            takenOff = false;

        }

        CalculateRollAndPitchAngles();
        thrust(thrustInput);
        if (takenOff)
        {
            pitch(pitchInput);
            roll(rollInput);
            yaw(yawInput);
        }
        clampVelocity();
        flightMode();
        sound();
        
        drone.rotation = Quaternion.Euler(new Vector3(currentForwardTilt, currentYRotation, currentSidewayTilt));

        for (int i = 0; i < totalPropellers; i++)
        {
            if (moveType.Equals("thrust"))
                propellerMovement(i, upForce, 0);

            if (moveType.Equals("forward"))
            {
                if (i == 0 || i == 1 || i == 6 || i == 7)
                    propellerMovement(i, upForce, 40);
                else
                    propellerMovement(i, upForce, -30);
            }
            if (moveType.Equals("backward"))
            {
                if (i == 0 || i == 1 || i == 6 || i == 7)
                    propellerMovement(i, upForce, -30);
                else
                    propellerMovement(i, upForce, 40);
            }
            if (moveType.Equals("left"))
            {
                if (i <= 3)
                    propellerMovement(i, upForce, 40);
                else
                    propellerMovement(i, upForce, -30);
            }
            if (moveType.Equals("right"))
            {
                if (i <= 3)
                    propellerMovement(i, upForce, -30);
                else
                    propellerMovement(i, upForce, 40);
            }

        }
    }


    //bool isGrounded()
    //{
    //    Debug.Log("Grounded!");
    //    return Physics.Raycast(drone.transform.position, -Vector3.up, distToGround + 0.1F);
    //}

    public float rotationSpeed = 0f;
    void propellerMovement(int index, float verticalForce, float propellerSpeed)
    {
        if (takenOff)
            rotationSpeed = drone.velocity.y + 40 + propellerSpeed;
        else
            rotationSpeed = 0;

        //Make two propellers a counter-rotating pair to cancel the torque
        if (index % 2 == 0)
            propellers[index].transform.Rotate(0, rotationSpeed, 0);
        else
            propellers[index].transform.Rotate(0, -rotationSpeed, 0);

        propellers[index].AddRelativeForce(Vector3.up * (verticalForce / 8));
    }



    void sound()
    {
        //if(drone.velocity.y >= -0.2f)
            droneSound.pitch = 0.8f + (drone.velocity.magnitude / 100);
        //else 
        //    droneSound.pitch = 0.8f - (drone.velocity.magnitude / 100);
    }


    /*
    *   Calculate the angular velocity w= 2pi / T
    *       T= 2pi*r / v
    *   Return: the angular velocity in rad/s
    */
    float getAngularVelocity()
    {
        float time = (2 * Mathf.PI) / velocity;
        //convert (rad per sec) to (round per minute); multiply with 9.55
        return ( ( (Mathf.PI * 2) / time ) * 9.55f) ;
    }






    /// <summary>
    /// The vertical Force created by the propellers
    /// </summary>
    /// <param name="thrustInput"></param>
    void thrust(float thrustInput)
    {
        if (takenOff)
        {
            droneSound.mute = false;
            crashSound.mute = false;
        }
        else
        {
            droneSound.mute = true;
            crashSound.mute = true;
        }

        if (Input.GetAxis("LeftJoystickVertical") != 0 )
            moveType = "thrust";

        if (Input.GetKey(KeyCode.I) || Input.GetAxis("LeftJoystickVertical") > 0)
        {
            thrustCoefficent = 1;
            upForce = thrustCoefficent * getAngularVelocity();
            takenOff = true;
        }
        else if (Input.GetKey(KeyCode.K) || Input.GetAxis("LeftJoystickVertical") < 0)
        {
            thrustCoefficent = -1;
            upForce = thrustCoefficent * getAngularVelocity();
        }

        else if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) 
            && (Input.GetAxis("LeftJoystickVertical") == 0))
                upForce = thrustInput;
        
    }



    /// <summary>
    /// The pitch moves the drone forward and backward
    /// </summary>
    /// <param name="pitchInput"></param>
    void pitch(float pitchInput)
    {
        

        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("RightJoystickVertical") != 0)
        {
            //Make sure the drone don't move done while steering
            //if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && (Input.GetAxis("LeftJoystickVertical") == 0))
            //    drone.velocity = new Vector3(drone.velocity.x, 0, drone.velocity.z);
            
            //Get the input of Keyboard or game controller
            if (Input.GetAxis("Vertical") != 0)
                pitchInput = Input.GetAxis("Vertical");
            else if(Input.GetAxis("RightJoystickVertical") != 0)
                pitchInput = Input.GetAxis("RightJoystickVertical");

            //Check the direction to simulate the propellers
            if (Input.GetAxis("Vertical") > 0 || Input.GetAxis("RightJoystickVertical") > 0)
                moveType = "forward";

            if (Input.GetAxis("Vertical") < 0 || Input.GetAxis("RightJoystickVertical") < 0)
                moveType = "backward";
            
            drone.AddRelativeForce(Vector3.forward * pitchInput * moveVelocity);
            currentForwardTilt = Mathf.SmoothDamp(currentForwardTilt, pitchInput, ref tiltVelocityForward, 0.1f);
            

        }
        else
        {
            //Propeller rotating with same velocity again
            moveType = "thrust";

            //Stop tilting
            currentForwardTilt = Mathf.SmoothDamp(currentForwardTilt, 0, ref tiltVelocityForward, 0.1f);
        }
    }



    /// <summary>
    /// Roll moves the drone left/right sideways
    /// The Propellers on side rotate faster than the other
    /// </summary>
    /// <param name="rollInput"></param>
    void roll(float rollInput)
    {
        

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("RightJoystickHorizontal") != 0)
        {
            //Make sure the drone don't move done while steering
            //if (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && (Input.GetAxis("LeftJoystickVertical") == 0))
            //    drone.velocity = new Vector3(drone.velocity.x, 0, drone.velocity.z);
            
            if (Input.GetAxis("Horizontal") != 0)
                rollInput = Input.GetAxis("Horizontal");
            else if(Input.GetAxis("RightJoystickHorizontal") != 0)
                rollInput = Input.GetAxis("RightJoystickHorizontal");

            if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("RightJoystickHorizontal") > 0)
                moveType = "right";

            if (Input.GetAxis("Horizontal") < 0 || Input.GetAxis("RightJoystickHorizontal") < 0)
                moveType = "left";

            drone.AddRelativeForce(Vector3.right * rollInput * moveVelocity);
            currentSidewayTilt = Mathf.SmoothDamp(currentSidewayTilt,  -1 * rollInput, ref tiltVelocitySideways, 0.1f);
        }
        else
        {
            //Propeller rotating with same velocity again
            //moveType = "thrust";

            //Stop tilting
            currentSidewayTilt = Mathf.SmoothDamp(currentSidewayTilt, 0, ref tiltVelocitySideways, 0.1f);
        }

    }



    /// <summary>
    /// Rotates the drone around the own axis
    /// </summary>
    /// <param name="yawInput"></param>
    void yaw(float yawInput)
    {
        if (takenOff)
        {
            //float horizontal = Input.GetAxis("Horizontal") * rotationFactor * Time.deltaTime;
            //transform.Rotate(0, 0, horizontal);

            if (Input.GetKey(KeyCode.J) || Input.GetAxis("LeftJoystickHorizontal") < 0)
                wantedYRotation -= yawSpeed;
            //drone.AddRelativeTorque(-1*Vector3.forward * rotationFactor, ForceMode.Force);

            //wantedYRotation -= rotateAmountByKeys;

            if (Input.GetKey(KeyCode.L) || Input.GetAxis("LeftJoystickHorizontal") > 0)
                wantedYRotation += yawSpeed;
            //drone.AddRelativeTorque(Vector3.forward * rotationFactor, ForceMode.Force);



            //Create delay to simulate animation
            currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0.4f);
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
