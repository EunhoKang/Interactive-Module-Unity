using UnityEngine;
using System;


namespace NuitrackSDK.NuitrackDemos
{
    public class GesturesVisualization : MonoBehaviour
    {
        ExceptionsLogger exceptionsLogger;

       void Update()
        {
            foreach (UserData user in NuitrackManager.Users)
            {
                if (user != null && user.GestureType != null)
                {
                    nuitrack.GestureType gesture = user.GestureType.Value;

                    string newEntry =
                        "User " + user.ID + ": " +
                        Enum.GetName(typeof(nuitrack.GestureType), (int)gesture);
                    exceptionsLogger.AddEntry(newEntry);
                }
            }
        }

        void Start()
        {
            exceptionsLogger = FindObjectOfType<ExceptionsLogger>();
        }
    }
}