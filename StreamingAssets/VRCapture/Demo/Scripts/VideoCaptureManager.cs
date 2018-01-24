using UnityEngine;
using UnityEngine.UI;

namespace VRCapture.Demo {

    public class VideoCaptureManager : MonoBehaviour {

        public Image recImage;
        public Text popup;
        private bool capturing;
        private bool finished = false;

        //Blinking
        private bool blink = false;
        private int counter = 1;
        private float blinkSpeed;


        void Start() {
            recImage.enabled = false;
            popup.enabled = false;
            VRCapture.Instance.RegisterSessionCompleteDelegate(HandleCaptureFinish);
            VRCapture.Instance.GetCaptureVideo(0).isEnabled = true;
            Application.runInBackground = true;
            capturing = false;
            popup.text = "Ready";
            blinkSpeed = (1f * Time.deltaTime);
        }


        void Update()
        {

            //Start capturing
            if ((Input.GetButtonDown("cross") || Input.GetKey(KeyCode.R))&& !capturing)
            {
                popup.enabled = true;

                popup.text = "Rec";
                VRCapture.Instance.BeginCaptureSession();
                print("Capture Start");
                capturing = true;
            }


            //Blinking
            if (capturing)
            {
                blinkSpeed += Time.deltaTime;
                if(blinkSpeed >= counter)
                {
                    recImage.enabled = true;
                    blinkSpeed = (1f * Time.deltaTime);
                }
                else
                    recImage.enabled = false;
            }



            //Processing
            if ((Input.GetButtonDown("circle") || Input.GetKey(KeyCode.T)) && capturing)
            {
                recImage.enabled = true;
                popup.text = "Ready";
                VRCapture.Instance.EndCaptureSession();
                print("Capture Stop");
                capturing = false;
                finished = true;
            }


            //Finished
            if (finished)
            {
                recImage.enabled = false;
                popup.enabled = false;
                popup.text = "Ready";
                finished = false;
            }


        }



        void HandleCaptureFinish() {
            print("Capture Finish");
        }
    }
}