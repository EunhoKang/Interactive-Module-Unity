using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZodiacSign : MonoBehaviour
{
    public string ZodiacSignName;
    public GameObject Glow;
    public List<Star> Stars;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    public void InitializeStars(){
        foreach(var star in Stars){
            star.SetParentZodiacSign(this);
        }
    }
    public void Emphasize(){
        Glow.SetActive(true);
    }
    public void Unemphasize(){
        Glow.SetActive(false);
    }
    public void Enlarge(Transform enlargedTransform){
        lastPosition = gameObject.transform.position;
        lastScale = gameObject.transform.localScale;
        gameObject.transform.position = enlargedTransform.position;
        gameObject.transform.localScale = enlargedTransform.localScale;
    }
    public void Reduct(){
        gameObject.transform.position = lastPosition;
        gameObject.transform.localScale = lastScale;
    }
}
