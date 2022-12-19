using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHide1 : MonoBehaviour
{
    public GameObject star;
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
            SetStarTrue();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            SetStarFalse();
        }
    }

    public void SetStarTrue()
    {
        star.SetActive(true);
        explode2.SetActive(false);
    }

    public void SetStarFalse()
    {
        star.SetActive(false);
        explode2.SetActive(true);
    }

}
