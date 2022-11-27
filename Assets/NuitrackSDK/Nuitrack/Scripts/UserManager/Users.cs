using System.Collections;
using System.Collections.Generic;
using System.Linq;

using nuitrack;


namespace NuitrackSDK
{
    public class Users : IEnumerable<UserData>
    {
        public delegate void UserHandler(UserData user);

        public event UserHandler OnUserEnter;
        public event UserHandler OnUserExit;

        /// <summary>
        /// Minimum allowed ID
        /// </summary>
        public static int MinID
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Maximum allowed ID
        /// </summary>
        public static int MaxID
        {
            get
            {
                return 6;
            }
        }

        readonly Dictionary<int, UserData> users = new Dictionary<int, UserData>();

        public IEnumerator<UserData> GetEnumerator()
        {
            return users.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// The number of users detected at the moment.
        /// </summary>
        public int Count
        {
            get
            {
                return users.Count;
            }
        }

        /// <summary>
        /// ID of the current user. <see cref="Current"/>
        /// </summary>
        public int CurrentUserID
        {
            get; private set;
        }

        /// <summary>
        /// UserData of the current user. Maybe null.
        ///
        /// <para>
        /// The queue rule is in effect, the current user is considered the first to enter the frame.
        /// When the current user leaves the frame, control is transferred to the next detected one)
        /// </para>
        /// </summary>
        public UserData Current
        {
            get
            {
                return GetUser(CurrentUserID) ?? null;
            }
        }

        /// <summary>
        /// Get a user by ID. Maybe null.
        /// </summary>
        /// <param name="userID">ID of the required user</param>
        /// <returns>User data, if the user exists otherwise null.</returns>
        public UserData GetUser(int userID)
        {
            if (users.ContainsKey(userID))
                return users[userID];
            else
                return null;
        }

        /// <summary>
        /// Get a list of all users in the form of a List<UserData>
        /// </summary>
        /// <returns>List of all users</returns>
        public List<UserData> GetList()
        {
            return new List<UserData>(users.Values);
        }

        UserData TryGetUser(int id, ref List<int> usersID)
        {
            if (!users.ContainsKey(id))
                users.Add(id, new UserData(id));

            usersID.Add(id);

            return users[id];
        }

        internal void UpdateData(SkeletonData skeletonData, HandTrackerData handTrackerData, GestureData gestureData, JsonInfo jsonInfo)
        {
            foreach (UserData user in this)
                user.Reset();

            List<int> oldUsersIDs = new List<int>(users.Keys);
            List<int> newUsersIDs = new List<int>();

            if (skeletonData != null)
                foreach (Skeleton skeleton in skeletonData.Skeletons)
                    TryGetUser(skeleton.ID, ref newUsersIDs).AddData(skeleton);

            if (handTrackerData != null)
                foreach (UserHands hands in handTrackerData.UsersHands)
                    TryGetUser(hands.UserId, ref newUsersIDs).AddData(hands);

            if (gestureData != null)
                foreach (Gesture gesture in gestureData.Gestures)
                    TryGetUser(gesture.UserID, ref newUsersIDs).AddData(gesture);

            if (jsonInfo != null && jsonInfo.Instances != null)
                foreach (Instances instances in jsonInfo.Instances)
                    if (!instances.face.IsEmpty)
                        TryGetUser(instances.id, ref newUsersIDs).AddData(instances.face);

            foreach (int userID in newUsersIDs)
                if (!oldUsersIDs.Contains(userID))
                    OnUserEnter?.Invoke(users[userID]);

            foreach (int userID in oldUsersIDs)
                if (!newUsersIDs.Contains(userID))
                {
                    OnUserExit?.Invoke(users[userID]);

                    users[userID].Dispose();
                    users.Remove(userID);
                }

            if (users.Count == 0)
                CurrentUserID = 0;
            else
            {
                if (CurrentUserID != 0 && !users.ContainsKey(CurrentUserID))
                    CurrentUserID = 0;

                if (CurrentUserID == 0)
                    CurrentUserID = users.Keys.First();
            }
        }
    }
}