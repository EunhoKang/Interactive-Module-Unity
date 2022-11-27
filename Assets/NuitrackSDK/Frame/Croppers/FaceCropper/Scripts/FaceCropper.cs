using UnityEngine;


namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/Cropper/Face Cropper")]
    public class FaceCropper : Cropper
    {
        protected override bool IsUserLost(UserData userData)
        {
            return userData == null || userData.Face == null;
        }

        protected override Rect GetFrameRect(UserData userData, float width, float height)
        {
            return userData.Face.ScreenRect(width, height);
        }
    }
}