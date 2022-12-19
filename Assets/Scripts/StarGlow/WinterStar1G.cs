using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterStar1G : MonoBehaviour
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
     if (Input.GetMouseButtonDown(0))
        {
            winterstar1gt();
        }

     if (Input.GetMouseButtonDown(1))
        {
            winterstar1gf();
        }
    }

    public void winterstar1gt()
    {
        mr.material.EnableKeyword("_EMISSION");
    }

    public void winterstar1gf()
    {
        mr.material.DisableKeyword("_EMISSION");
    }
}
