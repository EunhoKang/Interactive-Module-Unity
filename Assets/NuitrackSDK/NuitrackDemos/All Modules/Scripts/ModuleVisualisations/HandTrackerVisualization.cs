using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NuitrackSDK.NuitrackDemos
{
    public class HandTrackerVisualization : MonoBehaviour
    {
        [SerializeField] Transform handsContainer;
        [SerializeField] GameObject handUIPrefab;
        [SerializeField] float sizeNormal = 0, sizeClick = 0;
        [SerializeField] Color leftColor = Color.white, rightColor = Color.red;

        Dictionary<int, List<Image>> hands = new Dictionary<int, List<Image>>();

        void Update()
        {
            foreach (KeyValuePair<int, List<Image>> kvp in hands)
            {
                if (NuitrackManager.Users.GetUser(kvp.Key) == null)
                {
                    foreach (Image img in hands[kvp.Key])
                        img.enabled = false;
                }
            }

            foreach (UserData userData in NuitrackManager.Users)
            {
                if (!hands.ContainsKey(userData.ID))
                    CreateHands(userData.ID);

                ControllHand(userData.ID, 0, userData.LeftHand);
                ControllHand(userData.ID, 1, userData.RightHand);
            }
        }

        void CreateHands(int userID)
        {
            hands.Add(userID, new List<Image>());
            CreateHand(userID, leftColor);
            CreateHand(userID, rightColor);
        }

        void CreateHand(int userID, Color color)
        {
            Image image = Instantiate(handUIPrefab).GetComponent<Image>();

            image.transform.SetParent(handsContainer, false);        
            image.enabled = false;
            image.color = color;

            hands[userID].Add(image);
        }

        void ControllHand(int userID, int sideID, UserData.Hand hand)
        {
            if (hand == null)
                hands[userID][sideID].enabled = false;
            else
            {
                hands[userID][sideID].enabled = true;

                Vector2 pos = hand.Proj;

                hands[userID][sideID].rectTransform.anchorMin = pos;
                hands[userID][sideID].rectTransform.anchorMax = pos;
                hands[userID][sideID].rectTransform.sizeDelta = hand.Click ? new Vector2(sizeClick, sizeClick) : new Vector2(sizeNormal, sizeNormal);
            }
        }
    }
}