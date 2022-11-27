using System.Collections;
using UnityEngine;
using NuitrackSDK.Calibration;


namespace NuitrackSDK.Tutorials.ZombieVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/ZombieVR/Game Manager")]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] int maxEnemies = 100;

        [SerializeField] GameObject[] enemies;
        [SerializeField] Transform[] spawnPoints;

        float restartTime = 5;
        int enemiesCount = 0;

        bool gameStarted = false;

        private void OnEnable()
        {
            if (CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess += OnSuccessCalib;
        }

        private void OnSuccessCalib(Quaternion rotation)
        {
            if (!gameStarted)
            {
                gameStarted = true;
                InvokeRepeating("SpawnEnemy", 3, 0.2f);
            }
        }

        private void OnDisable()
        {
            if (CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess -= OnSuccessCalib;
        }

        void SpawnEnemy()
        {
            if (enemiesCount >= maxEnemies)
                return;

            float randomSize = Random.Range(0.2f, 0.3f); // zombie size

            enemies[Random.Range(0, enemies.Length)].transform.localScale = Vector3.one * randomSize; // set the zombie size
            Instantiate(enemies[Random.Range(0, enemies.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity); // spawn zombies 

            enemiesCount++;
        }

        public void GameOver()
        {
            StartCoroutine(Restart());
        }

        IEnumerator Restart()
        {
            yield return new WaitForSeconds(restartTime);
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}