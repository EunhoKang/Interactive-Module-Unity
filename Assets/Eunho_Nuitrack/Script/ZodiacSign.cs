using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZodiacSign : MonoBehaviour
{
    public string ZodiacSignName;
    public List<Star> Stars;
    private void Start(){
        foreach(var star in Stars){
            star.SetParentZodiacSign(this);
        }
    }
}
