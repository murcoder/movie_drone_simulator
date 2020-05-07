using UnityEngine;
using UnityEngine.VR;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityStandardAssets.ImageEffects;

namespace VRCapture.Demo
{

    public class LevelManager : MonoBehaviour
    {

        public Transform mainMenu, optionsMenu, keyboardMenu, ps3Menu;
        public Dropdown graphics;

        private GameObject cam;
        private AudioSource spaceAmbient;


        void Awake()
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera");
            spaceAmbient = cam.GetComponent<AudioSource>();
        }

        void Start()
        {
            //Creating listeners for the dropdown menu
            graphics.onValueChanged.AddListener(delegate { setGraphicQuality(graphics); });
        }



        public void LoadScene(string sceneName)
        {
            //cam.GetComponent<VignetteAndChromaticAberration>().intensity = 0.1f * Time.deltaTime;
            SceneManager.LoadScene(sceneName);
        }
        




        public void QuitGame()
        {
            Application.Quit();
        }

        //public void closingEffect()
        //{
        //    while (cam.GetComponent<VignetteAndChromaticAberration>().intensity != 1)
        //        cam.GetComponent<VignetteAndChromaticAberration>().intensity += (0.1f * Time.deltaTime);

        //    return;
        //}



        public void Options(bool clicked)
        {
            if (clicked == true)
            {
                optionsMenu.gameObject.SetActive(clicked);
                mainMenu.gameObject.SetActive(false);
            }
            else
            {
                optionsMenu.gameObject.SetActive(clicked);
                mainMenu.gameObject.SetActive(true);
            }
        }

        public void setGraphicQuality(Dropdown target)
        {
            if (target.value <= 5)
                QualitySettings.SetQualityLevel(target.value, false);
        }



        public void openVideoFolder()
        {
            //System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            //                      "/" + VRCaptureConfig.CAPTURE_FOLDER);
        }



        public void toggleKeyboard(bool clicked)
        {
            if(clicked == true)
            {
                keyboardMenu.gameObject.SetActive(clicked);
            }
            else
            {
                keyboardMenu.gameObject.SetActive(clicked);
            }
        }

        public void togglePs3(bool clicked)
        {
            if (clicked == true)
            {
                ps3Menu.gameObject.SetActive(clicked);
            }
            else
            {
                ps3Menu.gameObject.SetActive(clicked);
            }
        }

        public void music()
        {
            AudioSource mp3 = GameObject.Find("Canvas").GetComponent<AudioSource>();

            if (mp3.isPlaying)
            {
                spaceAmbient.Play();
                mp3.Stop();
            }
            else {
                spaceAmbient.Stop();
                mp3.Play();
            }
        }



        public void SetDropdownIndex(int index)
        {
            graphics.value = index;
        }
        void Destroy()
        {
            graphics.onValueChanged.RemoveAllListeners();
        }


    }
}
