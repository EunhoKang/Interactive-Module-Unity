using UnityEngine;

public class GlowVolume : MonoBehaviour
{
	public void ToggleGlow()
	{
		gameObject.SetActive(!gameObject.activeSelf);
	}
}
