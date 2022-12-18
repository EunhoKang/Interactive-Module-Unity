using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    private ZodiacSign parentZodiacSign;
    public ZodiacSign GetParentZodiacSign() {
        return parentZodiacSign;
    }
    public void SetParentZodiacSign(ZodiacSign zodiacSign) {
        parentZodiacSign = zodiacSign;
    }
}
