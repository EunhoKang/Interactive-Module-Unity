using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ZodiacSign : MonoBehaviour
{
    public string ZodiacSignName;
    public GameObject Glow;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    public void Emphasize(){
        Glow.SetActive(true);
    }
    public void Unemphasize(){
        Glow.SetActive(false);
    }
    public void Enlarge(Transform enlargedTransform){
        lastPosition = gameObject.transform.position;
        lastScale = gameObject.transform.localScale;
        transform.DOMove(enlargedTransform.position, 0.6f);
        transform.DOScale(enlargedTransform.localScale, 0.6f);
    }
    public void Reduct(){
        transform.DOMove( lastPosition, 0.6f );
        transform.DOScale( lastScale, 0.6f );
    }
}
