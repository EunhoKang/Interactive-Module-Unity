using UnityEngine;


namespace NuitrackSDK
{
    public abstract class TrackedUser : MonoBehaviour
    {
        [SerializeField, NuitrackSDKInspector]
        bool useCurrentUserTracker = true;

        [SerializeField, NuitrackSDKInspector]
        int userID = 1;

        /// <summary>
        /// If True, the current user tracker is used, otherwise the user specified by ID is used <see cref="UserID"/>
        /// </summary>
        public bool UseCurrentUserTracker
        {
            get
            {
                return useCurrentUserTracker;
            }
            set
            {
                useCurrentUserTracker = value;
            }
        }

        /// <summary>
        /// ID of the current user
        /// For the case when current user tracker <see cref="UseCurrentUserTracker"/> of is used, the ID of the active user will be returned
        /// If current user tracker is used and a new ID is set, tracking of the current user will stop
        /// </summary>
        public int UserID
        {
            get
            {
                if (UseCurrentUserTracker)
                    return NuitrackManager.Users.Current != null ? NuitrackManager.Users.Current.ID : 0;
                else
                    return userID;
            }
            set
            {
                if (value >= Users.MinID && value <= Users.MaxID)
                {
                    userID = value;

                    if (useCurrentUserTracker)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUserTracker = false;
                }
                else
                    throw new System.Exception(string.Format("The User ID must be within the bounds of [{0}, {1}]", Users.MinID, Users.MaxID));
            }
        }

        /// <summary>
        /// True if there is a control user.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return ControllerUser != null;
            }
        }

        /// <summary>
        /// The controler user. Maybe null
        /// </summary>
        public UserData ControllerUser
        {
            get
            {
                if (useCurrentUserTracker)
                    return NuitrackManager.Users.Current;
                else
                    return NuitrackManager.Users.GetUser(userID);
            }
        }
    }
}