using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHide1 : MonoBehaviour
{
    public GameObject sheep;
    public GameObject light1;
    public GameObject explode2;

    // Update is called once per frame
    void Update()
    {
        showhide();
    }

    void showhide()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetSheepTrue();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            sheep.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            explode2.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            explode2.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            light1.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            light1.SetActive(false);
        }

    }

    public void SetSheepTrue()
    {
        sheep.SetActive(true);
    }
}
