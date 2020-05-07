using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class record : MonoBehaviour {

    // The folder we place all screenshots inside.
    // If the folder exists we will append numbers to create an empty folder.
    public string folder = "ScreenshotMovieOutput";
    public int frameRate = 25;
    public int sizeMultiplier = 1;

    private bool recording;
    private string realFolder = "";

    void Start()
    {
        recording = false;
        // Set the playback framerate!
        // (real time doesn't influence time anymore)
        //Time.captureFramerate = frameRate;
        Debug.Log(Time.captureFramerate + " framerate: " + frameRate);
        // Find a folder that doesn't exist yet by appending numbers!
        realFolder = folder;
        int count = 1;
        while (System.IO.Directory.Exists(realFolder))
        {
            realFolder = folder + count;
            count++;
        }
        // Create the folder
        System.IO.Directory.CreateDirectory(realFolder);
    }

    void Update()
    {
        //Start capturing
        if (Input.GetButtonDown("cross"))
        {
            recording = true;
        }
        if (Input.GetButtonDown("circle"))
        {
            recording = false;
        }

    
        if (recording)
        {
            // name is "realFolder/pic0005.png"
            var name = string.Format("{0}/pic{1:D04}.png", realFolder, Time.frameCount);

            // Capture the screenshot
            Application.CaptureScreenshot(name, sizeMultiplier);
        }
    }
}
