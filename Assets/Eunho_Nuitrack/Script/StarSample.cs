using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSample : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} Entered");
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"{gameObject.name} Exited");
    }
}
