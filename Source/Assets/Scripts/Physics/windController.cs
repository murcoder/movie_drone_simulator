using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class windController : MonoBehaviour
{

    public Rigidbody rb;
    public Text value;

    private bool rightWind = false;
    private bool leftWind = false;
    private bool forwardWind = false;
    private bool backwardWind = false;
    private float strength = 10;
    private AudioSource windHowl;


    void Awake()
    {
        value.text = GameObject.Find("Strenght").GetComponent<Slider>().value.ToString();
        windHowl = GameObject.Find("windHowl").GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        //Adding force to the desired rigidbody in global space
        rb.AddForce(checkWindSelection());
    }


    /**
    /   Get user selection of wind direction
    **/
    public void ToggleValueChange(Toggle tgl)
    {
        string tmp = tgl.name;

        if (tgl.isOn)
        {
            if (tmp.Equals("rightWind"))
                rightWind = true;
            else if (tmp.Equals("leftWind"))
                leftWind = true;

            if (tmp.Equals("forwardWind"))
                forwardWind = true;
            else if (tmp.Equals("backwardWind"))
                backwardWind = true;
            
        }
        else
        {
            if (tmp.Equals("rightWind"))
                rightWind = false;
            else if (tmp.Equals("leftWind"))
                leftWind = false;

            if (tmp.Equals("forwardWind"))
                forwardWind = false;
            else if (tmp.Equals("backwardWind"))
                backwardWind = false;

        }

        //Debug.Log("TMP: " + tmp + "|| rightWind: " + rightWind + "; leftWind: " + leftWind
        //    + "; forwardWind: " + forwardWind + "; backwardWind: " + backwardWind);
    }


    /**
    /   Set the strength value to the user selection
    **/
    public void setStrenght(float s)
    {
        strength = s;
        value.text = Mathf.Round(strength).ToString();
    }


    /**
    /   Create the wind force depending on the user selection
    /   Returns: the desired vector3
    **/
    private Vector3 checkWindSelection()
    {
        Vector3 result;
        float x = 0;
        float z = 0;


        if (rightWind)
            x = Random.Range(0, strength);
        else if (leftWind)
            x = -(Random.Range(0, strength));

        if (forwardWind)
            z = Random.Range(0, strength);
        else if (backwardWind)
            z = -(Random.Range(0, strength));


        if (rightWind || leftWind || forwardWind || backwardWind)
        {
            if(!windHowl.isPlaying)
                windHowl.Play();

        }
        else {
            if (windHowl.isPlaying)
                windHowl.Stop();
        }

        result = new Vector3(x, 0, z);
        return result;
    }

}
