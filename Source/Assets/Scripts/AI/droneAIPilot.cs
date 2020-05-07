using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof(droneController))]
    public class droneAIPilot : MonoBehaviour
    {
        // This script represents an AI 'pilot' capable of flying the plane towards a designated target.
        // It sends the equivalent of the inputs that a user would send to the Aeroplane controller.
        [SerializeField]
        private float m_RollSensitivity = .2f;         // How sensitively the AI applies the roll controls
        [SerializeField]
        private float m_PitchSensitivity = .5f;        // How sensitively the AI applies the pitch controls
        [SerializeField]
        private float m_LateralWanderDistance = 5;     // The amount that the plane can wander by when heading for a target
        [SerializeField]
        private float m_LateralWanderSpeed = 0.11f;    // The speed at which the plane will wander laterally
        [SerializeField]
        private float m_MaxClimbAngle = 45;            // The maximum angle that the AI will attempt to make plane can climb at
        [SerializeField]
        private float m_MaxRollAngle = 45;             // The maximum angle that the AI will attempt to u
        [SerializeField]
        private float m_SpeedEffect = 0.01f;           // This increases the effect of the controls based on the plane's speed.
        [SerializeField]
        private float m_TakeoffHeight = 4;            // the AI will fly straight and only pitch upwards until reaching this height
        [SerializeField]
        private Transform m_Target;                    // the target to fly towards

        private droneController m_droneController;  // The aeroplane controller that is used to move the plane
        private float m_RandomPerlin;                       // Used for generating random point on perlin noise so that the plane will wander off path slightly
        private bool m_TakenOff;                            // Has the plane taken off yet


        // setup script properties
        private void Awake()
        {
            // get the reference to the aeroplane controller, so we can send move input to it and read its current state.
            m_droneController = GetComponent<droneController>();

            // pick a random perlin starting point for lateral wandering
            m_RandomPerlin = Random.Range(0f, 100f);
        }


        // reset the object to sensible values
        public void Reset()
        {
            m_TakenOff = false;
        }


        // fixed update is called in time with the physics system update
        private void FixedUpdate()
        {
            if (m_Target != null)
            {
                // make the drone wander from the path, useful for making the AI seem more human, less robotic.
                Vector3 targetPos = m_Target.position +
                                    transform.right *
                                    (Mathf.PerlinNoise(Time.time * m_LateralWanderSpeed, m_RandomPerlin) * 2 - 1) *
                                    m_LateralWanderDistance;

                // adjust the yaw and pitch towards the target
                Vector3 localTarget = transform.InverseTransformPoint(targetPos);

                //Returns the angle beetween the x-Axis and the Point(x,z)
                float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);

                //Returns the angle beetween the x-Axis and the Point(y,z)
                float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);


                // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
                targetAnglePitch = Mathf.Clamp(targetAnglePitch, -m_MaxClimbAngle * Mathf.Deg2Rad,
                                               m_MaxClimbAngle * Mathf.Deg2Rad);

                // calculate the difference between current pitch and desired pitch
                float changePitch = targetAnglePitch - m_droneController.PitchAngle;

                // AI always applies gentle forward throttle
                const float thrustInput = 0.5f;

                // AI applies elevator control (pitch, rotation around x) to reach the target angle
                float pitchInput = changePitch * m_PitchSensitivity;

                // clamp the planes roll
                float desiredRoll = Mathf.Clamp(targetAngleYaw, -m_MaxRollAngle * Mathf.Deg2Rad, m_MaxRollAngle * Mathf.Deg2Rad);
                float yawInput = 0;
                float rollInput = 0;
                if (!m_TakenOff)
                {
                    // If the planes altitude is above m_TakeoffHeight we class this as taken off
                    if (m_droneController.currentAltitude > m_TakeoffHeight)
                    {
                        m_TakenOff = true;
                    }
                }
                else
                {
                    // now we have taken off to a safe height, we can use the rudder and ailerons to yaw and roll
                    yawInput = targetAngleYaw;
                    rollInput = -(m_droneController.RollAngle - desiredRoll) * m_RollSensitivity;
                }

                // adjust how fast the AI is changing the controls based on the speed. Faster speed = faster on the controls.
                float currentSpeedEffect = 1 + (m_droneController.moveVelocity * m_SpeedEffect);
                rollInput *= currentSpeedEffect;
                pitchInput *= currentSpeedEffect;
                yawInput *= currentSpeedEffect;

                Debug.Log("roll: " + rollInput + "; pitch: " + pitchInput + "; yaw: " + yawInput + "; thrust: " + thrustInput);
                // pass the current input to the plane
                m_droneController.move(rollInput, pitchInput, yawInput, thrustInput);
            }
            else
            {
                // no target set, send zeroed input to the plane
                m_droneController.move(0, 0, 0, 0);
            }
        }


        // allows other scripts to set the plane's target
        public void SetTarget(Transform target)
        {
            m_Target = target;
        }
    }
}
