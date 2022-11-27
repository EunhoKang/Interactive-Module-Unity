using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.Collections.Generic;
using System.Linq;

using NuitrackSDK;
using NuitrackSDKEditor.Avatar;


namespace NuitrackSDKEditor.Poses
{
    public class SkeletonPoseView : System.IDisposable
    {
        public delegate void BoneHandler(nuitrack.JointType jointType);
        public delegate void BoneRotationHandler(nuitrack.JointType jointType, Quaternion rotation);
        public delegate void BoneToleranceHandler(nuitrack.JointType jointType, float tolerance);

        public event BoneHandler OnBoneSetActive;
        public event BoneHandler OnBoneDelete;

        public event BoneRotationHandler OnBoneRotate;
        public event BoneToleranceHandler OnBoneToleranceChanged;

        readonly ColorTheme colorTheme = new ColorTheme()
        {
            mainColor = new Color(1f, 0.5f, 0f),
            disableColor = new Color(0.4f, 0.4f, 0.4f)
        };

        readonly Color selectColor = Color.white;
        readonly Color hoverColor = Color.black;

        const float jointSphereSize = 0.065f;

        const float boneLength = 0.2f;

        GameObject dude;
        Animator animator;

        readonly List<nuitrack.JointType> jointsMask = null;
        readonly Dictionary<nuitrack.JointType, Quaternion> rotationsOffset = null;

        readonly Dictionary<Vector3, Vector3[]> aldDirect = new Dictionary<Vector3, Vector3[]>()
        {
            { Vector3.up, new Vector3[] { Vector3.left, Vector3.forward } },
            { Vector3.forward, new Vector3[] { Vector3.up, Vector3.left } },
            { Vector3.left, new Vector3[] { Vector3.forward, Vector3.up } },

            { Vector3.down, new Vector3[] { Vector3.right, Vector3.back } },
            { Vector3.back, new Vector3[] { Vector3.down, Vector3.right } },
            { Vector3.right, new Vector3[] { Vector3.back, Vector3.down } },
        };

        public nuitrack.JointType SelectedJoint
        {
            get;
            set;
        } = nuitrack.JointType.None;

        /// <summary>
        /// <see cref="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/AvatarPreview.cs#L326"/>
        /// </summary>
        public SkeletonPoseView(Transform spawnTransform, List<nuitrack.JointType> jointsMask, ColorTheme colorTheme = null)
        {
            this.jointsMask = jointsMask;
            this.colorTheme = colorTheme ?? this.colorTheme;

            GameObject dudeObject = (GameObject)EditorGUIUtility.Load("Avatar/DefaultAvatar.fbx");

            dude = Object.Instantiate(dudeObject, spawnTransform);
            dude.hideFlags = HideFlags.HideAndDontSave;

            StageUtility.PlaceGameObjectInCurrentStage(dude);

            animator = dude.GetComponent<Animator>();
            animator.enabled = false;

            rotationsOffset = new Dictionary<nuitrack.JointType, Quaternion>();

            foreach (nuitrack.JointType jointType in jointsMask)
            {
                Transform jointTransform = animator.GetBoneTransform(jointType.ToUnityBones());
                rotationsOffset.Add(jointType, jointTransform.rotation);
            }
        }

        public void Dispose()
        {
            if (dude != null)
            {
                Object.DestroyImmediate(dude);

                rotationsOffset.Clear();

                dude = null;
                animator = null;
            }
        }

        public void DrawScenePose(List<nuitrack.JointType> activeJoints, Dictionary<nuitrack.JointType, Quaternion> jointsRotation, Dictionary<nuitrack.JointType, float> jointTolerance)
        {
            foreach (nuitrack.JointType jointType in jointsMask)
            {
                bool isActive = activeJoints.Contains(jointType);

                Transform transform = animator.GetBoneTransform(jointType.ToUnityBones());
                transform.rotation = rotationsOffset[jointType] * jointsRotation[jointType];

                using (new HandlesColor(isActive ? colorTheme.mainColor : colorTheme.disableColor))
                {
                    List<nuitrack.JointType> childList = jointType.GetChilds();

                    List<Transform> childs = childList.Select(k => animator.GetBoneTransform(k.ToUnityBones())).ToList();

                    int controllerID = GUIUtility.GetControlID(dude.name.GetHashCode(), FocusType.Passive);
                    DrawBoneController(controllerID, transform, childs, jointType, jointSphereSize);

                    if (SelectedJoint == jointType)
                    {
                        Quaternion newRotation = Handles.RotationHandle(jointsRotation[jointType], transform.position).normalized;

                        if (!Mathf.Approximately(Quaternion.Dot(jointsRotation[jointType], newRotation), 1))
                            OnBoneRotate?.Invoke(jointType, newRotation);

                        using (new HandlesColor(Color.yellow))
                        {
                            float newTolerance = DrawCone(transform.position, jointType.GetNormalDirection(), jointsRotation[jointType], jointTolerance[jointType]);

                            if (!Mathf.Approximately(newTolerance, jointTolerance[jointType]))
                                OnBoneToleranceChanged?.Invoke(jointType, newTolerance);
                        }
                    }
                }
            }
        }

        public void DrawGUIInspector()
        {
            GUIContent toObjectGUIContent = new GUIContent("Centered in view", EditorGUIUtility.IconContent("SceneViewCamera").image);

            if (GUILayout.Button(toObjectGUIContent))
                SkeletonUtils.CenteredInView(dude.transform);
        }

        void DrawBoneController(int controllerID, Transform boneTransform, List<Transform> childs, nuitrack.JointType jointType, float size)
        {
            childs ??= new List<Transform>();

            Event e = Event.current;

            //We divide the size by 2, since strange behavior is detected when an element falls into the selection.
            //The size of the visual element is set by the diameter, and the selection area by the radius.

            Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size / 2, EventType.Layout);

            switch (e.GetTypeForControl(controllerID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controllerID && e.button == 0)
                    {
                        // Respond to a press on this handle. Drag starts automatically.
                        GUIUtility.hotControl = controllerID;
                        GUIUtility.keyboardControl = controllerID;

                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controllerID && e.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();

                        OnBoneSetActive?.Invoke(jointType);
                    }
                    break;

                case EventType.Repaint:
                    Color handlesColor = Handles.color;

                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controllerID)
                        handlesColor = Color.Lerp(Handles.color, hoverColor, 0.5f);

                    if (SelectedJoint == jointType)
                        handlesColor = Color.Lerp(handlesColor, selectColor, 0.5f);

                    using (new HandlesColor(handlesColor))
                    {
                        Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size, EventType.Repaint);

                        foreach (Transform child in childs)
                            SkeletonUtils.DrawBone(boneTransform.position, child.position);
                    }
                    break;
                case EventType.KeyDown:
                    if ((e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete) && GUIUtility.keyboardControl == controllerID)
                    {
                        GUIUtility.keyboardControl = 0;
                        e.Use();

                        OnBoneDelete?.Invoke(jointType);
                    }
                    break;
            }
        }

        float DrawCone(Vector3 position, Vector3 jointDirection, Quaternion rotation, float tolerance)
        {
            Vector3 up = (rotation * aldDirect[jointDirection][0]).normalized;
            Vector3 right = (rotation * aldDirect[jointDirection][1]).normalized;

            Vector3 shiftPoint = rotation * jointDirection * boneLength;

            tolerance = DrawConePoint(position, shiftPoint, up, tolerance);
            tolerance = DrawConePoint(position, shiftPoint, -up, tolerance);

            tolerance = DrawConePoint(position, shiftPoint, right, tolerance);
            tolerance = DrawConePoint(position, shiftPoint, -right, tolerance);

            float lightDisc = boneLength * tolerance * Mathf.Tan(Mathf.Acos(tolerance));

            Vector3 endPosition = position + shiftPoint * tolerance;
            Handles.DrawWireDisc(endPosition, (endPosition - position).normalized, lightDisc);

            return tolerance;
        }

        float DrawConePoint(Vector3 startPoint, Vector3 shiftPoint, Vector3 pointDelta, float dot)
        {
            float lightDisc = boneLength * dot * Mathf.Tan(Mathf.Acos(dot));

            Vector3 endPosition = startPoint + shiftPoint * dot;
            Vector3 dotPosition = endPosition + pointDelta * lightDisc;

            Handles.DrawLine(startPoint, dotPosition);

            float size = HandleUtility.GetHandleSize(dotPosition);
            Vector3 newPosition = Handles.Slider(dotPosition, (dotPosition - endPosition).normalized, size * 0.05f, Handles.DotHandleCap, 0f);

            return Vector3.Dot((endPosition - startPoint).normalized, (newPosition - startPoint).normalized);
        }
    }
}