using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class demo : MonoBehaviour
{
    public GameObject[] objects;

    public void loadScene1()
    {
        SceneManager.LoadScene("set1");
    }

    public void loadScene2()
    {
        SceneManager.LoadScene("set2");
    }



    public void switchModel()
    {
        GameObject newObj = null;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].activeSelf)
            {
                objects[i].SetActive(false);

                if (i + 1 < objects.Length)
                {
                    objects[i + 1].SetActive(true);
                    newObj = objects[i + 1];

                }
                else {
                    objects[0].SetActive(true);
                    newObj = objects[0];

                }
            }

        }

        GameObject tmp = GameObject.FindGameObjectWithTag("MainCamera");
        tmp.GetComponent<moveAroundObject>().target = newObj.transform;

    }
}
