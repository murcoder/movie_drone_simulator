using UnityEngine;
using UnityEngine.VR;
using System.Collections;


public class cameraController : MonoBehaviour {

    [Header("Values")]
    public Camera[] cameras;
    public Camera preview;
    private int selectedCamera = 0;
    private int totalCameras = 0;
    private bool VRMode;
    



	void Start () {
        totalCameras = cameras.Length;
        Switch(0); //Default camera
        VRMode = false;

    }
	


	void Update () {


        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(LoadDevice("oculus"));

            if (!VRMode)
                VRMode = true;
            else
                VRMode = false;

        }

        //Handle the camera switching
        if (!VRMode && (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("square")))
        {
            selectedCamera++;
            if (selectedCamera >= totalCameras)
                selectedCamera = 0;
            Switch(selectedCamera);
        }

        if (VRMode)
        {
            cameras[3].enabled = true;
            cameras[3].transform.position = new Vector3(-1,0,3);
 


        }


    }


    IEnumerator LoadDevice(string newDevice)
    {
        VRSettings.LoadDeviceByName(newDevice);
        yield return null;
        VRSettings.enabled = true;
        Debug.Log("VR: loaded " + newDevice + ": " + VRSettings.enabled);
    }



    void Switch(int selectedCamera){

        for(int i=0; i< totalCameras; i++)
        {
            //Activate the correct camera
            if (i != selectedCamera)
                cameras[i].enabled = false;
            else
                cameras[i].enabled = true;
        }

        //Show in all cameraview a preview of the video camera, exept for the video camera itself
        if (cameras[selectedCamera].tag.Equals("VideoCamera"))
            preview.enabled = false;
        else
            preview.enabled = true;

        //For the video camera turn the gui off
        if (cameras[selectedCamera].tag.Equals("VideoCamera"))
            GUI.enabled = false;
        
    }





    /**
    /   Calculating the current horizontal field of view
    /   @Return: the horizontal Field of view depending on the aspect ratio
    **/
    private float horizontalFOV()
    {
        //Get pilotCamera
        float vFOVrad  = cameras[0].fieldOfView * Mathf.Deg2Rad;
        float cameraHeightAt1 = Mathf.Tan(vFOVrad * 0.5f);
        float hFOVrad  = Mathf.Atan(cameraHeightAt1 * cameras[0].aspect) * 2;
        float hFOV = hFOVrad * Mathf.Rad2Deg;
        return hFOV;
    }

}
