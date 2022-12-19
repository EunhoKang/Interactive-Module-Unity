using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterStar2G : MonoBehaviour
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            winterstar2gt();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            winterstar2gf();
        }
    }

    public void winterstar2gt()
    {
        mr.material.EnableKeyword("_EMISSION");
    }

    public void winterstar2gf()
    {
        mr.material.DisableKeyword("_EMISSION");
    }
}
