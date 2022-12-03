using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shower : MonoBehaviour
{
    public GameObject sheep;

    public void OpenSheep()
    {
        if(sheep != null)
        {
            bool isActive = sheep.activeSelf;
            sheep.SetActive(!isActive);
        }
    }

}
