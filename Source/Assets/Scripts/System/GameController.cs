using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using VRCapture;

namespace DigitalRuby.SimpleLUT
{

    public class GameController : MonoBehaviour
    {

        public Rigidbody rb;
        public Text velocity;
        public Text speed;
        public Text yrotation;
        public Text altitude;
        public Dropdown PilotDropdown;
        public Dropdown VideoDropdown;

        private Camera cam;
        private GameObject hud;


        void Awake()
        {
            hud = GameObject.FindGameObjectWithTag("HUD");
        }

        void Start()
        {
            //Creating listeners for the dropdown menu
            PilotDropdown.onValueChanged.AddListener(delegate { setPilotLense(PilotDropdown); });
            VideoDropdown.onValueChanged.AddListener(delegate { setVideoCameraFilter(VideoDropdown); });
        }


        void Update()
        {
            checkInput();
            setHUD();

        }


        /// <summary>
        /// Handle the general User input
        /// </summary>
        void checkInput()
        {
            //DEBUGGING
            if (Input.GetKey(KeyCode.B)) {
                Debug.Log("****NEW FFMPEG Build Folder: " + Application.streamingAssetsPath + "/VRCapture/FFmpeg/Win/" + "ffmpeg.exe");
                Debug.Log("***NEW SAVE FOLDER: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/VRCapture/");
                Debug.Log("***OLD FFMPEG WIN PATH: " + VRCaptureConfig.FFMPEG_WIN_PATH);
                Debug.Log("***OLD CAPTURE FOLDER: " + VRCaptureConfig.CAPTURE_FOLDER);
            }

            //If V is pressed, toggle VRSettings.enabled
            //if (Input.GetKeyDown(KeyCode.V))
            //{
            //    VRSettings.LoadDeviceByName("oculus");
            //    VRSettings.enabled = true;
            //}

            //Hit Start on ps3 controller to go back to menu
            if (Input.GetButtonDown("Submit") || Input.GetKey(KeyCode.Escape))
                SceneManager.LoadScene("menu");

            //Hit Select on ps3 controller to switch the scene
            if (Input.GetButtonDown("Cancel") || Input.GetKey(KeyCode.F1))
                SceneManager.LoadScene("sceneSelector");

            //Turn off/on the HUD
            if (Input.GetButtonDown("triangle") || Input.GetKey(KeyCode.F2)) {

                if (hud.activeSelf)
                    hud.SetActive(false);
                else
                    hud.SetActive(true);

             }

        }

        void setHUD()
        {
            velocity.text = "Velocity: " + rb.velocity.ToString();

            //Unity measures in m/s; to convert to km/h multiply by 3.6
            speed.text = "Speed: " + Mathf.Round(rb.velocity.magnitude * 3.6f) + " km/h";

            yrotation.text = "Y-Rotation: " + Mathf.Round(rb.transform.rotation.eulerAngles.y) + "°";

            altitude.text = "Altitude: " + Mathf.Round(rb.GetComponent<droneController>().currentAltitude) + " m";
        }



        public void restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("resetPosition");
        }




        //Dropdownmenu handler
        private void myDropdownValueChangedHandler(Dropdown target)
        {
            Debug.Log(target.value);
        }



        private void setPilotLense(Dropdown target)
        {
            cam = GameObject.Find("PilotCamera").GetComponent<Camera>();

            if (target.value == 0)
                cam.fieldOfView = 60;
            else if (target.value == 1)
                cam.fieldOfView = 92;
            else if (target.value == 2)
                cam.fieldOfView = 88;
        }


        private void setVideoCameraFilter(Dropdown target)
        {
            setFilter(GameObject.Find("VideoCamera").GetComponent<Camera>(), target.value);
            setFilter(GameObject.Find("VideoCameraPreview").GetComponent<Camera>(), target.value);
        }


        private void setFilter(Camera cam, int filter)
        {
            //None
            if (filter == 0)
            {
                resetVideoFilter(cam);
            }
            //Vivid
            if (filter == 1)
            {
                resetVideoFilter(cam);
                cam.GetComponent<SimpleLUT>().Saturation = 0.5f;
                cam.GetComponent<SimpleLUT>().Brightness = 0.2f;
                cam.GetComponent<SimpleLUT>().Contrast = 0.2f;
            }
            //Black and White
            if (filter == 2)
            {
                resetVideoFilter(cam);
                cam.GetComponent<SimpleLUT>().Saturation = -1f;
                cam.GetComponent<SimpleLUT>().Hue = 40;
                cam.GetComponent<SimpleLUT>().Contrast = 0.2f;
            }
            //Cartoon
            if (filter == 3)
            {
                resetVideoFilter(cam);
                cam.GetComponent<SimpleLUT>().Brightness = 0.15f;
                cam.GetComponent<SimpleLUT>().Sharpness = 1f;
                cam.GetComponent<SimpleLUT>().Saturation = -0.2f;
            }

        }

        void resetVideoFilter(Camera cam)
        {
            cam.GetComponent<SimpleLUT>().Saturation = 0f;
            cam.GetComponent<SimpleLUT>().Brightness = 0f;
            cam.GetComponent<SimpleLUT>().Contrast = 0f;
            cam.GetComponent<SimpleLUT>().Sharpness = 0f;
            cam.GetComponent<SimpleLUT>().Hue = 0f;
        }

        public void SetDropdownIndex(int index)
        {
            PilotDropdown.value = index;
            VideoDropdown.value = index;
        }
        void Destroy()
        {
            PilotDropdown.onValueChanged.RemoveAllListeners();
            VideoDropdown.onValueChanged.RemoveAllListeners();
        }



    }

}

