using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IUsesStreamReader" />
    /// <summary>
    /// Updates tracking pose values from the stream reader to the transformed referenced in this script.
    /// </summary>
    public class CharacterRigController : MonoBehaviour, IUsesStreamReader
    {
        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount to smoothing when returning to the start pose for the character when AR tracking is lost.")]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("Enable controller driving eye bones pose.")]
        bool m_DriveEyes = true;

        [SerializeField]
        [Tooltip("Left eye bone transform")]
        Transform m_LeftEye;

        [SerializeField]
        [Tooltip("Right eye bone transform")]
        Transform m_RightEye;

        [SerializeField]
        [Tooltip("Local offset distance for the eye look target")]
        float m_EyeLookDistance = -0.5f;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of smoothing to apply to eye movement")]
        float m_EyeSmoothing = 0.2f;

        [SerializeField]
        [Tooltip("Corrects the right eye look direction if z is negative.")]
        bool m_RightEyeNegZ;

        [SerializeField]
        [Tooltip("Corrects the left eye look direction if z is negative.")]
        bool m_LeftEyeNegZ;

        [SerializeField]
        [Tooltip("Max amount of x and y movement for the eyes.")]
        Vector2 m_EyeAngleRange = new Vector2(30, 45);

        [SerializeField]
        [Tooltip("Enable controller driving head bone pose.")]
        bool m_DriveHead = true;

        [SerializeField]
        [Tooltip("Head bone transform")]
        Transform m_HeadBone;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of smoothing to apply to head movement")]
        float m_HeadSmoothing = 0.1f;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of influence the AR head anchor pose has on the head bone.")]
        float m_HeadFollowAmount = 0.6f;

        [SerializeField]
        [Tooltip("Enable controller driving neck bone pose.")]
        bool m_DriveNeck = true;

        [SerializeField]
        [Tooltip("Neck bone transform")]
        Transform m_NeckBone;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of influence the AR head anchor pose has on the neck bone.")]
        float m_NeckFollowAmount = 0.4f;

        int m_EyeLookDownLeftIndex;
        int m_EyeLookDownRightIndex;
        int m_EyeLookInLeftIndex;
        int m_EyeLookInRightIndex;
        int m_EyeLookOutLeftIndex;
        int m_EyeLookOutRightIndex;
        int m_EyeLookUpLeftIndex;
        int m_EyeLookUpRightIndex;

        float m_EyeLookDownLeft;
        float m_EyeLookDownRight;
        float m_EyeLookInLeft;
        float m_EyeLookInRight;
        float m_EyeLookOutLeft;
        float m_EyeLookOutRight;
        float m_EyeLookUpLeft;
        float m_EyeLookUpRight;

        Transform m_ARHeadPose;
        Transform m_ARNeckPose;
        Transform m_AREyePose;

        Transform m_EyePoseLookAt;
        Transform m_EyeRightPoseLookAt;
        Transform m_EyeLeftPoseLookAt;

        Transform m_LocalizedHeadParent;
        Transform m_LocalizedHeadRot;
        Transform m_OtherThing;
        Transform m_OtherLook;

        Pose m_HeadStartPose;
        Pose m_NeckStartPose;
        Pose m_RightEyeStartPose;
        Pose m_LeftEyeStartPose;

        Quaternion m_LastHeadRotation;
        Quaternion m_LastNeckRotation;

        Quaternion m_BackwardRot = Quaternion.Euler(0, 180, 0);

        IStreamSettings m_LastStreamSettings;

        public IStreamReader streamReader { private get; set; }

        public Transform headBone { get { return m_HeadBone != null ? m_HeadBone : transform; } }

        public bool driveEyes { get { return m_DriveEyes; } }
        public bool driveHead { get { return m_DriveHead; } }
        public bool driveNeck { get { return m_DriveNeck; } }

        [NonSerialized]
        [HideInInspector]
        public Transform[] animatedBones = new Transform [4];

        void Start()
        {
            SetupCharacterRigController();
        }

        void Update()
        {
            var streamSource = streamReader.streamSource;
            if (streamSource == null || !streamSource.active)
                return;

            var streamSettings = streamReader.streamSource.streamSettings;
            if (streamSettings != m_LastStreamSettings)
                UpdateBlendShapeIndices(streamSettings);

            InterpolateBlendShapes();
        }

        void LateUpdate()
        {
            var streamSource = streamReader.streamSource;
            if (streamSource == null || !streamSource.active)
                return;

            UpdateBoneTransforms();
        }

        public void UpdateBlendShapeIndices(IStreamSettings settings)
        {
            m_LastStreamSettings = settings;
            m_EyeLookDownLeftIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookDownLeft);
            m_EyeLookDownRightIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookDownRight);
            m_EyeLookInLeftIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookInLeft);
            m_EyeLookInRightIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookInRight);
            m_EyeLookOutLeftIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookOutLeft);
            m_EyeLookOutRightIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookOutRight);
            m_EyeLookUpLeftIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookUpLeft);
            m_EyeLookUpRightIndex = settings.GetLocationIndex(BlendShapeUtils.EyeLookUpRight);
        }

        public void SetupCharacterRigController()
        {
            Debug.Log("Start avatar Setup");

            if (m_DriveEyes)
            {
                if (m_LeftEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Left Eye Bone returning NULL!");
                    m_DriveEyes = false;
                }

                if (m_RightEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Right Eye Bone returning NULL!");
                    m_DriveEyes = false;
                }
            }

            if (m_DriveHead && m_HeadBone == null)
            {
                Debug.LogWarning("Drive Head is set but Head Bone returning NULL!");
                m_DriveHead = false;
            }

            if (m_DriveNeck && m_NeckBone == null)
            {
                Debug.LogWarning("Drive Neck is set but Neck Bone returning NULL!");
                m_DriveNeck = false;
            }

            Pose headWorldPose;
            Pose headLocalPose;
            if (m_DriveHead)
            {
                // ReSharper disable once PossibleNullReferenceException
                headWorldPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
                headLocalPose = new Pose(m_HeadBone.localPosition, m_HeadBone.localRotation);
            }
            else if (m_DriveNeck)
            {
                // ReSharper disable once PossibleNullReferenceException
                headWorldPose = new Pose(m_NeckBone.position, m_NeckBone.rotation);
                headLocalPose = new Pose(m_NeckBone.localPosition, m_NeckBone.localRotation);
            }
            else
            {
                headWorldPose = new Pose(transform.position, transform.rotation);
                headLocalPose = new Pose(transform.localPosition, transform.localRotation);
            }

            Pose neckWorldPose;
            Pose neckLocalPose;
            if (m_DriveNeck)
            {
                // ReSharper disable once PossibleNullReferenceException
                neckWorldPose = new Pose(m_NeckBone.position, m_NeckBone.rotation);
                neckLocalPose = new Pose(m_NeckBone.localPosition, m_NeckBone.localRotation);
            }
            else if (m_HeadBone)
            {
                // ReSharper disable once PossibleNullReferenceException
                neckWorldPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
                neckLocalPose = new Pose(m_HeadBone.localPosition, m_HeadBone.localRotation);
            }
            else
            {
                neckWorldPose = new Pose(transform.position, transform.rotation);
                neckLocalPose = new Pose(transform.localPosition, transform.localRotation);
            }

            Pose eyeLeftWorldPose;
            Pose eyeLeftLocalPose;
            Pose eyeRightWorldPose;
            Pose eyeRightLocalPose;
            if (m_DriveEyes)
            {
                // ReSharper disable once PossibleNullReferenceException
                eyeLeftWorldPose = new Pose(m_LeftEye.position, m_LeftEye.rotation);
                eyeLeftLocalPose = new Pose(m_LeftEye.localPosition, m_LeftEye.localRotation);
                // ReSharper disable once PossibleNullReferenceException
                eyeRightWorldPose = new Pose(m_RightEye.position, m_RightEye.rotation);
                eyeRightLocalPose = new Pose(m_RightEye.localPosition, m_RightEye.localRotation);
            }
            else
            {
                eyeLeftWorldPose = new Pose(headWorldPose.position, headWorldPose.rotation);
                eyeLeftLocalPose = new Pose(headLocalPose.position, headLocalPose.rotation);
                eyeRightWorldPose = new Pose(headWorldPose.position, headWorldPose.rotation);
                eyeRightLocalPose = new Pose(headLocalPose.position, headLocalPose.rotation);
            }

            // Set Head Look Rig
            var headPoseObject = new GameObject("head_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_ARHeadPose = headPoseObject.transform;

            m_HeadStartPose = new Pose(headLocalPose.position, headLocalPose.rotation);

            m_ARHeadPose.SetPositionAndRotation(headWorldPose.position, Quaternion.identity);
            var headOffset = new GameObject("head_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            headOffset.SetPositionAndRotation(headWorldPose.position, headWorldPose.rotation);
            headOffset.SetParent(transform, true);
            m_ARHeadPose.SetParent(headOffset, true);
            m_ARHeadPose.localRotation = Quaternion.identity;

            // Set Neck Look Rig
            var neckPoseObject = new GameObject("neck_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_ARNeckPose = neckPoseObject.transform;

            m_NeckStartPose = new Pose(neckLocalPose.position, neckLocalPose.rotation);

            m_ARNeckPose.SetPositionAndRotation(neckWorldPose.position, Quaternion.identity);
            var neckOffset = new GameObject("neck_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            neckOffset.SetPositionAndRotation(neckWorldPose.position, neckWorldPose.rotation);
            neckOffset.SetParent(transform, true);
            m_ARNeckPose.SetParent(neckOffset, true);
            m_ARNeckPose.localRotation = Quaternion.identity;

            // Set Eye Look Rig
            var eyePoseObject = new GameObject("eye_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_AREyePose = eyePoseObject.transform;
            var eyePoseLookObject = new GameObject("eye_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyePoseLookAt = eyePoseLookObject.transform;
            m_EyePoseLookAt.SetParent(m_AREyePose);
            m_EyePoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance;

            // Eye Center Look
            m_AREyePose.SetPositionAndRotation(Vector3.Lerp(eyeRightWorldPose.position, eyeLeftWorldPose.position, 0.5f), transform.rotation);
            var eyeOffset = new GameObject("eye_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            eyeOffset.position = m_AREyePose.position;
            eyeOffset.SetParent(m_HeadBone != null ? m_HeadBone : transform, true);

            m_AREyePose.SetParent(eyeOffset, true);

            // Eye Right Look
            m_RightEyeStartPose = new Pose(eyeRightLocalPose.position, eyeRightLocalPose.rotation);
            var rightEyeOffset = eyeRightWorldPose.position - m_AREyePose.position;
            var eyeRightPoseLookObject = new GameObject("eye_right_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyeRightPoseLookAt = eyeRightPoseLookObject.transform;
            m_EyeRightPoseLookAt.SetParent(m_AREyePose);
            if (!m_RightEyeNegZ)
                m_EyeRightPoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance + rightEyeOffset;
            else
                m_EyeRightPoseLookAt.localPosition = Vector3.back * m_EyeLookDistance + rightEyeOffset;

            // Eye Left Look
            m_LeftEyeStartPose = new Pose(eyeLeftLocalPose.position, eyeLeftLocalPose.rotation);
            var leftEyeOffset = eyeLeftWorldPose.position - m_AREyePose.position ;
            var eyeLeftPoseLookObject = new GameObject("eye_left_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyeLeftPoseLookAt = eyeLeftPoseLookObject.transform;
            m_EyeLeftPoseLookAt.SetParent(m_AREyePose);
            if(!m_LeftEyeNegZ)
                m_EyeLeftPoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance + leftEyeOffset;
            else
                m_EyeLeftPoseLookAt.localPosition = Vector3.back * m_EyeLookDistance + leftEyeOffset;

            m_AREyePose.rotation = Quaternion.identity;

            // Other strange rig stuff
            m_LocalizedHeadParent = new GameObject("loc_head_parent"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            var locHeadOffset = new GameObject("loc_head_offset"){ hideFlags = HideFlags.HideAndDontSave};
            locHeadOffset.transform.SetParent(m_LocalizedHeadParent);
            m_LocalizedHeadRot = new GameObject("loc_head_rot"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            m_LocalizedHeadRot.SetParent(locHeadOffset.transform);
            m_OtherLook = new GameObject("other_look"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            m_OtherLook.SetParent(m_LocalizedHeadRot.transform);
            m_OtherLook.localPosition = Vector3.forward * 0.25f;

            var otherThingOffset = new GameObject("other_thing_offset"){ hideFlags = HideFlags.HideAndDontSave};
            otherThingOffset.transform.SetParent(m_LocalizedHeadParent);
            otherThingOffset.transform.rotation = transform.rotation;
            m_OtherThing = new GameObject("other_thing"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            m_OtherThing.SetParent(otherThingOffset.transform);

            if (m_HeadBone != null)
                animatedBones[0] = m_HeadBone;

            if (m_NeckBone != null)
                animatedBones[1] = m_NeckBone;

            if (m_LeftEye != null)
                animatedBones[2] = m_LeftEye;

            if (m_RightEye != null)
                animatedBones[3] = m_RightEye;
        }

        public void ResetBonePoses()
        {
            if (m_DriveEyes)
            {
                m_RightEye.localPosition = m_RightEyeStartPose.position;
                m_RightEye.localRotation = m_RightEyeStartPose.rotation;

                m_LeftEye.localPosition = m_LeftEyeStartPose.position;
                m_LeftEye.localRotation = m_LeftEyeStartPose.rotation;
            }

            if (m_DriveHead)
            {
                m_HeadBone.localPosition = m_HeadStartPose.position;
                m_HeadBone.localRotation = m_HeadStartPose.rotation;
            }

            if (m_DriveNeck)
            {
                m_NeckBone.localPosition = m_NeckStartPose.position;
                m_NeckBone.localRotation = m_NeckStartPose.rotation;
            }
        }

        public void InterpolateBlendShapes(bool force = false)
        {
            var streamSource = streamReader.streamSource;
            if (streamSource == null)
                return;

            if (force || streamReader.trackingActive)
            {
                LocalizeFacePose();

                var buffer = streamReader.blendShapesBuffer;
                m_EyeLookDownLeft = Mathf.Lerp(buffer[m_EyeLookDownLeftIndex], m_EyeLookDownLeft, m_EyeSmoothing);
                m_EyeLookInLeft = Mathf.Lerp(buffer[m_EyeLookInLeftIndex], m_EyeLookInLeft, m_EyeSmoothing);
                m_EyeLookOutLeft = Mathf.Lerp(buffer[m_EyeLookOutLeftIndex], m_EyeLookOutLeft, m_EyeSmoothing);
                m_EyeLookUpLeft = Mathf.Lerp(buffer[m_EyeLookUpLeftIndex], m_EyeLookUpLeft, m_EyeSmoothing);

                m_EyeLookDownRight = Mathf.Lerp(buffer[m_EyeLookDownRightIndex], m_EyeLookDownRight, m_EyeSmoothing);
                m_EyeLookInRight = Mathf.Lerp(buffer[m_EyeLookInRightIndex], m_EyeLookInRight, m_EyeSmoothing);
                m_EyeLookOutRight = Mathf.Lerp(buffer[m_EyeLookOutRightIndex], m_EyeLookOutRight, m_EyeSmoothing);
                m_EyeLookUpRight = Mathf.Lerp(buffer[m_EyeLookUpRightIndex], m_EyeLookUpRight, m_EyeSmoothing);

                var leftEyePitch = Quaternion.AngleAxis((m_EyeLookUpLeft - m_EyeLookDownLeft) * m_EyeAngleRange.x, Vector3.right);
                var leftEyeYaw = Quaternion.AngleAxis((m_EyeLookInLeft - m_EyeLookOutLeft) * m_EyeAngleRange.y, Vector3.up);
                var leftEyeRot = leftEyePitch * leftEyeYaw;

                var rightEyePitch = Quaternion.AngleAxis((m_EyeLookUpRight - m_EyeLookDownRight) * m_EyeAngleRange.x, Vector3.right);
                var rightEyeYaw = Quaternion.AngleAxis((m_EyeLookOutRight - m_EyeLookInRight) * m_EyeAngleRange.y, Vector3.up);
                var rightEyeRot = rightEyePitch * rightEyeYaw;

                m_AREyePose.localRotation = Quaternion.Slerp(leftEyeRot, rightEyeRot, 0.5f);

                var headRot = m_LocalizedHeadRot.localRotation;
                var neckRot = headRot;

                headRot = Quaternion.Slerp(m_HeadStartPose.rotation, headRot, m_HeadFollowAmount);
                m_ARHeadPose.localRotation = Quaternion.Slerp(headRot, m_LastHeadRotation, m_HeadSmoothing);
                m_LastHeadRotation = m_ARHeadPose.localRotation;

                neckRot = Quaternion.Slerp(m_NeckStartPose.rotation, neckRot, m_NeckFollowAmount);
                m_ARNeckPose.localRotation = Quaternion.Slerp(neckRot, m_LastNeckRotation, m_HeadSmoothing);
                m_LastNeckRotation = m_ARNeckPose.localRotation;
            }
            else
            {
                if (m_DriveEyes)
                {
                    m_AREyePose.localRotation = Quaternion.Slerp(Quaternion.identity, m_AREyePose.localRotation, m_TrackingLossSmoothing);
                    m_ARHeadPose.localRotation = Quaternion.Slerp(Quaternion.identity, m_ARHeadPose.localRotation, m_TrackingLossSmoothing);
                }

                m_LastHeadRotation = m_ARHeadPose.localRotation;
            }

            if (force)
                UpdateBoneTransforms();
        }

        void LocalizeFacePose()
        {
            var headPose = streamReader.headPose;
            m_LocalizedHeadParent.position = headPose.position;
            m_LocalizedHeadParent.LookAt(streamReader.cameraPose.position);

            m_LocalizedHeadRot.rotation = headPose.rotation * m_BackwardRot;
            m_OtherThing.LookAt(m_OtherLook, m_OtherLook.up);
        }

        void UpdateBoneTransforms()
        {
            if (m_DriveEyes)
            {
                if (m_RightEyeNegZ)
                    m_RightEye.LookAt(m_EyeRightPoseLookAt, Vector3.down);
                else
                    m_RightEye.LookAt(m_EyeRightPoseLookAt);

                if (m_LeftEyeNegZ)
                    m_LeftEye.LookAt(m_EyeLeftPoseLookAt, Vector3.down);
                else
                    m_LeftEye.LookAt(m_EyeLeftPoseLookAt);
            }

            if (m_DriveHead)
                m_HeadBone.rotation = m_ARHeadPose.rotation;

            if (m_DriveNeck )
                m_NeckBone.rotation = m_ARNeckPose.rotation;
        }
    }
}
