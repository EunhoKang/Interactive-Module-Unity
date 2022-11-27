using System.Collections.Generic;
using UnityEngine;


namespace NuitrackSDK.Face
{
    [AddComponentMenu("NuitrackSDK/Face/UI Point Face")]
    public class UIPointsFace : MonoBehaviour
    {
        [SerializeField] GameObject pointPrefab;
        [SerializeField] RectTransform spawnRect;
        
        readonly Dictionary<int, List<RectTransform>> facePoints = new Dictionary<int, List<RectTransform>>();
        
        readonly float landmarkCount = 31;
        readonly float eyesCount = 2;

        void Start()
        {
            for (int userID = Users.MinID; userID <= Users.MaxID; userID++)
            {
                facePoints.Add(userID, new List<RectTransform>());

                for (int i = 0; i < landmarkCount + eyesCount; i++)
                {
                    RectTransform point = Instantiate(pointPrefab, spawnRect).GetComponent<RectTransform>();
                    point.gameObject.SetActive(false);

                    facePoints[userID].Add(point);
                }
            }
        }

        void DisplayPoints(List<RectTransform> points, bool visible)
        {
            foreach (RectTransform point in points)
                point.gameObject.SetActive(visible);
        }

        void Update()
        {
            foreach (KeyValuePair<int, List<RectTransform>> pointData in facePoints)
            {
                if (NuitrackManager.Users.GetUser(pointData.Key) == null)
                    DisplayPoints(pointData.Value, false);
            }

            foreach (UserData user in NuitrackManager.Users)
            {
                if (user.Face != null)
                {
                    DisplayPoints(facePoints[user.ID], true);

                    for (int i = 0; i < landmarkCount; i++)
                    {
                        Vector2 point = user.Face.landmark[i];
                        Vector2 position = new Vector2(point.x * spawnRect.rect.width, spawnRect.rect.height - point.y * spawnRect.rect.height);
                        facePoints[user.ID][i].anchoredPosition = position - spawnRect.rect.size / 2;
                    }

                    Vector2 positionLeftEye = new Vector2(user.Face.left_eye.x * spawnRect.rect.width, spawnRect.rect.height - user.Face.left_eye.y * spawnRect.rect.height);
                    facePoints[user.ID][32].anchoredPosition = positionLeftEye - spawnRect.rect.size / 2;
                    facePoints[user.ID][32].GetComponent<UnityEngine.UI.Image>().color = Color.red;

                    Vector2 positionRightEye = new Vector2(user.Face.right_eye.x * spawnRect.rect.width, spawnRect.rect.height - user.Face.right_eye.y * spawnRect.rect.height);
                    facePoints[user.ID][31].anchoredPosition = positionRightEye - spawnRect.rect.size / 2;
                    facePoints[user.ID][31].GetComponent<UnityEngine.UI.Image>().color = Color.green;
                }
                else
                    DisplayPoints(facePoints[user.ID], false);
            }
        }
    }
}