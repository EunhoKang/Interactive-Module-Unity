using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ZodiacSign : MonoBehaviour
{
    public string ZodiacSignName;
    public MeshRenderer Glow;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    public void Emphasize(){
        Glow.material.EnableKeyword("_EMISSION");
    }
    public void Unemphasize(){
        Glow.material.DisableKeyword("_EMISSION");
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
