using UnityEngine;


namespace NuitrackSDK.Tutorials.MotionCapture
{
    public interface IRecordable
    {
        void TakeSnapshot(float deltaTime);

        AnimationClip GetClip { get; }
    }
}