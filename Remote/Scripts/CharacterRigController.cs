using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    public class CharacterRigController : MonoBehaviour, IUseStreamSettings, IUseReaderActive, IUseReaderBlendShapes,
        IUseReaderHeadPose, IUseReaderCameraPose
    {
        [SerializeField]
        [Range(0f, 1f)]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        bool m_DriveEyes = true;

        [SerializeField]
        Transform m_LeftEye;

        [SerializeField]
        Transform m_RightEye;

        [SerializeField]
        float m_EyeLookDistance = -0.5f;

        [Range(0f, 1f)]
        [SerializeField]
        float m_EyeSmoothing = 0.2f;

        [SerializeField]
        bool m_RightEyeNegZ;

        [SerializeField]
        bool m_LeftEyeNegZ;

        [SerializeField]
        Vector2 m_EyeAngleRange = new Vector2(30, 45);

        [SerializeField]
        bool m_DriveHead = true;

        [SerializeField]
        Transform m_HeadBone;

        [Range(0f, 1f)]
        [SerializeField]
        float m_HeadSmoothing = 0.1f;

        [SerializeField]
        [Range(0f, 1f)]
        float m_HeadFollowAmount = 0.6f;

        [SerializeField]
        bool m_DriveNeck = true;

        [SerializeField]
        Transform m_NeckBone;

        [SerializeField]
        [Range(0f, 1f)]
        float m_NeckFollowAmount = 0.4f;

        bool m_HasEyes;
        bool m_HasHead;
        bool m_HasNeck;

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

        protected IStreamSettings streamSettings { get { return getStreamSettings(); } }
        public Func<IStreamSettings> getStreamSettings { get; set; }
        IStreamSettings readerStreamSettings { get { return getReaderStreamSettings(); } }
        public Func<IStreamSettings> getReaderStreamSettings { get; set; }
        bool isReaderStreamActive { get { return isStreamActive(); } }
        public Func<bool> isStreamActive { get; set; }
        bool isReaderTrackingActive { get { return isTrackingActive(); } }
        public Func<bool> isTrackingActive { get; set; }
        float[] readerBlendShapesBuffer { get { return getBlendShapesBuffer(); } }
        public Func<float[]> getBlendShapesBuffer { get; set; }
        Pose readerHeadPose { get { return getHeadPose(); } }
        public Func<Pose> getHeadPose { get; set; }
        Pose readerCameraPose { get { return getCameraPose(); } }
        public Func<Pose> getCameraPose { get; set; }

        public Transform headBone { get { return m_HeadBone ?? transform; } }

        public bool driveEyes { get { return m_DriveEyes; } }
        public bool driveHead { get { return m_DriveHead; } }
        public bool driveNeck { get { return m_DriveNeck; } }

        [NonSerialized]
        [HideInInspector]
        public bool connected;

        [NonSerialized]
        [HideInInspector]
        public Transform[] animatedBones = new Transform [4];

        public void OnStreamSettingsChange()
        {
            SetupBlendShapeIndices();
        }

        void Start()
        {
            SetupCharacterRigController();
        }

        public void SetupBlendShapeIndices()
        {
            // TODO should try to use active settings
            m_EyeLookDownLeftIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookDownLeft);
            m_EyeLookDownRightIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookDownRight);
            m_EyeLookInLeftIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookInLeft);
            m_EyeLookInRightIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookInRight);
            m_EyeLookOutLeftIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookOutLeft);
            m_EyeLookOutRightIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookOutRight);
            m_EyeLookUpLeftIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookUpLeft);
            m_EyeLookUpRightIndex = readerStreamSettings.GetLocationIndex(BlendShapeUtils.EyeLookUpRight);
        }

        public void SetupCharacterRigController()
        {
            Debug.Log("Start avatar Setup");

            if (m_DriveEyes)
            {
                if (m_LeftEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Left Eye Bone returning NULL!");
                    return;
                }

                if (m_RightEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Right Eye Bone returning NULL!");
                    return;
                }
            }

            if (m_LeftEye != null && m_RightEye != null)
                m_HasEyes = true;

            if (m_DriveHead && m_HeadBone == null)
            {
                Debug.LogWarning("Drive Head is set but Head Bone returning NULL!");
                return;
            }

            if (m_HeadBone != null)
                m_HasHead = true;

            if (m_DriveNeck && m_NeckBone == null)
            {
                Debug.LogWarning("Drive Neck is set but Neck Bone returning NULL!");
                return;
            }

            if (m_NeckBone != null)
                m_HasNeck = true;

            Pose headWorldPose;
            Pose headLocalPose;
            if (m_HasHead)
            {
                // ReSharper disable once PossibleNullReferenceException
                headWorldPose = new Pose(m_HeadBone.position, m_HeadBone.rotation);
                headLocalPose = new Pose(m_HeadBone.localPosition, m_HeadBone.localRotation);
            }
            else if (m_HasNeck)
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
            if (m_HasNeck)
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
            if (m_HasEyes)
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
            if (m_HeadBone != null)
                eyeOffset.SetParent(m_HeadBone, true);
            else
                eyeOffset.SetParent(transform, true);

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
            var leftEyeOffset = eyeRightWorldPose.position - m_AREyePose.position ;
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
            else
                animatedBones[0] = transform;

            if (m_NeckBone != null)
                animatedBones[1] = m_NeckBone;
            else
                animatedBones[1] = transform;

            if (m_LeftEye != null)
                animatedBones[2] = m_LeftEye;
            else
                animatedBones[2] = transform;

            if (m_RightEye != null)
                animatedBones[3] = m_RightEye;
            else
                animatedBones[3] = transform;
        }

        void LocalizeFacePose()
        {
            m_LocalizedHeadParent.position = readerHeadPose.position;
            m_LocalizedHeadParent.LookAt(readerCameraPose.position);

            m_LocalizedHeadRot.rotation = readerHeadPose.rotation * m_BackwardRot;
            m_OtherThing.LookAt(m_OtherLook, m_OtherLook.up);
        }

        void Update()
        {
            if (!connected || !isReaderStreamActive)
                return;

            InterpolateBlendShapes();
        }

        public void InterpolateBlendShapes(bool force = false)
        {
            if (!force && !isReaderStreamActive)
                return;

            if (force || isReaderTrackingActive)
            {
                LocalizeFacePose();

                m_EyeLookDownLeft = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookDownLeftIndex], m_EyeLookDownLeft, m_EyeSmoothing);
                m_EyeLookInLeft = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookInLeftIndex], m_EyeLookInLeft, m_EyeSmoothing);
                m_EyeLookOutLeft = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookOutLeftIndex], m_EyeLookOutLeft, m_EyeSmoothing);
                m_EyeLookUpLeft = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookUpLeftIndex], m_EyeLookUpLeft, m_EyeSmoothing);

                m_EyeLookDownRight = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookDownRightIndex], m_EyeLookDownRight, m_EyeSmoothing);
                m_EyeLookInRight = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookInRightIndex], m_EyeLookInRight, m_EyeSmoothing);
                m_EyeLookOutRight = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookOutRightIndex], m_EyeLookOutRight, m_EyeSmoothing);
                m_EyeLookUpRight = Mathf.Lerp(readerBlendShapesBuffer[m_EyeLookUpRightIndex], m_EyeLookUpRight, m_EyeSmoothing);

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
                m_AREyePose.localRotation = Quaternion.Slerp(Quaternion.identity, m_AREyePose.localRotation, m_TrackingLossSmoothing);
                m_ARHeadPose.localRotation = Quaternion.Slerp(Quaternion.identity, m_ARHeadPose.localRotation, m_TrackingLossSmoothing);
                m_LastHeadRotation = m_ARHeadPose.localRotation;
            }

            if (force)
                UpdateBoneTransforms();
        }

        public void ResetBonePoses()
        {
            if (m_DriveEyes && m_HasEyes)
            {
                m_RightEye.localPosition = m_RightEyeStartPose.position;
                m_RightEye.localRotation = m_RightEyeStartPose.rotation;

                m_LeftEye.localPosition = m_LeftEyeStartPose.position;
                m_LeftEye.localRotation = m_LeftEyeStartPose.rotation;
            }

            if (m_DriveHead && m_HasHead)
            {
                m_HeadBone.localPosition = m_HeadStartPose.position;
                m_HeadBone.localRotation = m_HeadStartPose.rotation;
            }

            if (m_DriveNeck && m_HasNeck)
            {
                m_NeckBone.localPosition = m_NeckStartPose.position;
                m_NeckBone.localRotation = m_NeckStartPose.rotation;
            }
        }

        void UpdateBoneTransforms()
        {
            if (m_DriveEyes && m_HasEyes)
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

            if (m_DriveHead && m_HasHead)
                m_HeadBone.rotation = m_ARHeadPose.rotation;

            if (m_DriveNeck && m_HasNeck)
                m_NeckBone.rotation = m_ARNeckPose.rotation;
        }

        void LateUpdate()
        {
            if (isReaderStreamActive)
                UpdateBoneTransforms();
        }
    }
}
