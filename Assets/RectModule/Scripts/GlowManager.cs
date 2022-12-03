using UnityEngine;

public class GlowManager : MonoBehaviour
{
	public Material Glow, NonGlow;
	public bool     IsGlowing = true;

	public void ToggleGlow()
	{
		gameObject.GetComponent<Renderer>().material = IsGlowing ? NonGlow : Glow;
		IsGlowing                                    = !IsGlowing;
	}
}
