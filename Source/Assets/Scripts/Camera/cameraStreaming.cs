using UnityEngine;
using System.IO;




/// <summary>
/// 1. Read the screen pixels to an empty Texture2D using Texture2D.ReadPixels
/// 2. Encode the Texture2D using EncodeToJPG or EncodeToPNG
/// 3. Send the encoded bytes through a socket to the device
/// 4. On the device: read the bytes into an image(using a combination of ByteArrayInputStream and BitmapFactory for example)
/// </summary>

public class cameraStreaming : MonoBehaviour
    {

    public bool isCapturing;
    Texture2D tex;
    int width;
    int height;
    public Renderer display;


    void Start()
    {
        // Create a texture the size of the screen, RGB24 format
        width = Screen.width;
        height = Screen.height;
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {

            
            OnPostRender();
            Debug.Log("B");

        }
    }



    /// <summary>
    /// OnPostRender() is called after a camera finished rendering the scene.
    /// </summary>
    public void OnPostRender()
    {

        if (isCapturing)
        {
            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            tex.Apply();
            //display.material.mainTexture = tex;

            //Encodes this texture into PNG format and saves it into a byte-array
            byte[] bytes;
            bytes = tex.EncodeToPNG();
            Object.Destroy(tex);

            // For testing purposes, write to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
            Debug.Log("Saved: " + Application.dataPath + "/../SavedScreen.png");

            isCapturing = false;
        }
    }



}
