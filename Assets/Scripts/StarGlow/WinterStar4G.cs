using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterStar4G : MonoBehaviour
{

    MeshRenderer mr;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            winterstar4gt();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            winterstar4gf();
        }
    }

    public void winterstar4gt()
    {
        mr.material.EnableKeyword("_EMISSION");
    }

    public void winterstar4gf()
    {
        mr.material.DisableKeyword("_EMISSION");
    }
}
