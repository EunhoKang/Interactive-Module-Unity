using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterStar3G : MonoBehaviour
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
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            winterstar3gt();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            winterstar3gf();
        }
    }

    public void winterstar3gt()
    {
        mr.material.EnableKeyword("_EMISSION");
    }

    public void winterstar3gf()
    {
        mr.material.DisableKeyword("_EMISSION");
    }
}
