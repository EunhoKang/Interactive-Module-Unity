using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGate : MonoBehaviour
{
    private bool isEnlargementMode;
    private ZodiacSign enlargedZodiacSign;
    private List<ZodiacSign>[] zodiacSigns= new List<ZodiacSign>[4];
    #region Stars
    public GameObject sheep;
    public List<ZodiacSign> SpringZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> SummerZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> AutumnZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> WinterZodiacSigns = new List<ZodiacSign>();
    public Transform EnlargeTransform;
    #endregion
    #region Effects
    public GameObject cityLightEffect;
    public GameObject explodeEffect;
    #endregion
    #region Unity Events
    void Start(){
        isEnlargementMode = false;
        zodiacSigns[0] = SpringZodiacSigns;
        zodiacSigns[1] = SummerZodiacSigns;
        zodiacSigns[2] = AutumnZodiacSigns;
        zodiacSigns[3] = WinterZodiacSigns;
        for ( int i = 0; i < 4; ++i ) {
            foreach ( var zodiacSign in zodiacSigns[i] ) {
                zodiacSign.gameObject.SetActive(true);
                zodiacSign.InitializeStars();
            }
        }
    }
    #endregion
    #region API
    public void EmphasizeStar(){
        ZodiacSign[] targetZodiacSigns = TrackManager.Instance.GetTargetZodiacSigns();
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.Unemphasize();
        foreach ( var zodiacSign in targetZodiacSigns) if ( zodiacSign != null ) zodiacSign.Emphasize();
    }
    public void EnlargeZodiacSign(){
        if( isEnlargementMode ) return;
        isEnlargementMode = true;
        ZodiacSign targetZodiacSign = TrackManager.Instance.GetTargetZodiacSign();
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.gameObject.SetActive(false);
        targetZodiacSign.gameObject.SetActive(true);
        targetZodiacSign.Enlarge(EnlargeTransform);
        enlargedZodiacSign = targetZodiacSign;
    }
    public void ReductZodiacSign(){
        if( ! isEnlargementMode ) return;
        isEnlargementMode = false;
        enlargedZodiacSign.Reduct();
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.gameObject.SetActive(true);
    }
    public void SetCityLightTrue()
    {
        cityLightEffect.SetActive(true);
    }
    public void SetCityLightFalse()
    {
        cityLightEffect.SetActive(false);
    }

    public void SetSheepTrue()
    {
        sheep.SetActive(true);
    }
    public void SetSheepFalse()
    {
        sheep.SetActive(false);
    }
    public void SetExplodeTrue()
    {
        explodeEffect.SetActive(true);
    }
    public void SetExplodeFalse()
    {
        explodeEffect.SetActive(false);
    }
    #endregion
}
