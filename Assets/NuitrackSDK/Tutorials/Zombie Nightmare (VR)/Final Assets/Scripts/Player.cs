using UnityEngine;


namespace NuitrackSDK.Tutorials.ZombieVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/ZombieVR/Player")]
    public class Player : MonoBehaviour
    {
        float health = 100;
        [SerializeField] UnityEngine.UI.Image healthBar;

        public void GetDamage(float damage)
        {
            if (health <= 0)
                return;

            health -= damage;

            if (health <= 0)
            {
                health = 0;
                FindObjectOfType<GameManager>().GameOver();
            }

            healthBar.fillAmount = health / 100;
        }
    }
}