using System;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    public class CharacterRigController : MonoBehaviour, IUseStreamSettings, IUseReaderActive, IUseReaderBlendShapes,
        IUseReaderHeadPose, IUseReaderCameraPose
    {
        [Range(0f, 1f)]
        [SerializeField]
        float m_EyeSmoothing = 0.2f;

        [Range(0f, 1f)]
        [SerializeField]
        float m_HeadSmoothing = 0.1f;

        [SerializeField]
        Transform m_LeftEye;

        [SerializeField]
        Transform m_RightEye;

        [SerializeField]
        Vector2 m_EyeAngleRange = new Vector2(30, 45);

        [SerializeField]
        [Range(0f, 1f)]
        float m_TrackingLossSmoothing = 0.1f;

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

        [SerializeField]
        Transform m_HeadBone;
        [SerializeField]
        Transform m_NeckBone;
        Transform m_ARHeadPose;
        Transform m_ARNeckPose;
        Transform m_AREyePose;

        Transform m_EyePoseLookAt;
        Transform m_EyeRightPoseLookAt;
        Transform m_EyeLeftPoseLookAt;

        [SerializeField]
        [Range(0f, 1f)]
        float m_HeadFollowAmount = 0.6f;

        [SerializeField]
        [Range(0f, 1f)]
        float m_NeckFollowAmount = 0.4f;

        Pose m_HeadStartPose;
        Pose m_NeckStartPose;
        Pose m_RightEyeStartPose;
        Pose m_LeftEyeStartPose;

        [SerializeField]
        bool m_RightEyeNegZ;

        [SerializeField]
        bool m_LeftEyeNegZ;

        Quaternion m_LastHeadRotation;
        Quaternion m_LastNeckRotation;

        Transform m_LocalizedHeadParent;
        Transform m_LocalizedHeadRot;
        Transform m_OtherThing;
        Transform m_OtherLook;

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

        [NonSerialized]
        [HideInInspector]
        public bool connected;

        [NonSerialized]
        [HideInInspector]
        public Transform[] animatedBones = new Transform [4];

        public void OnStreamSettingsChangeChange()
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
            m_EyeLookDownLeftIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookDownLeft);
            m_EyeLookDownRightIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookDownRight);
            m_EyeLookInLeftIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookInLeft);
            m_EyeLookInRightIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookInRight);
            m_EyeLookOutLeftIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookOutLeft);
            m_EyeLookOutRightIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookOutRight);
            m_EyeLookUpLeftIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookUpLeft);
            m_EyeLookUpRightIndex = readerStreamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookUpRight);
        }

        public void SetupCharacterRigController()
        {
            Debug.Log("Start avatar Setup");

            if (m_LeftEye == null)
            {
                Debug.LogError("Left Eye Bone returning NULL!");
                enabled = false;
                return;
            }

            if (m_RightEye == null)
            {
                Debug.LogError("Right Eye Bone returning NULL!");
                enabled = false;
                return;
            }

            if (m_HeadBone == null)
            {
                Debug.LogError("Head Bone returning NULL!");
                enabled = false;
                return;
            }


            if (m_NeckBone == null)
            {
                Debug.LogError("Neck Bone returning NULL!");
                enabled = false;
                return;
            }

            // Set Head Look Rig
            var headPoseObject = new GameObject("head_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_ARHeadPose = headPoseObject.transform;

            m_HeadStartPose = new Pose(m_HeadBone.localPosition, m_HeadBone.localRotation);

            m_ARHeadPose.SetPositionAndRotation(m_HeadBone.position, Quaternion.identity);
            var headOffset = new GameObject("head_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            headOffset.SetPositionAndRotation(m_HeadBone.position, m_HeadBone.rotation);
            headOffset.SetParent(transform, true);
            m_ARHeadPose.SetParent(headOffset, true);
            m_ARHeadPose.localRotation = Quaternion.identity;

            // Set Neck Look Rig
            var neckPoseObject = new GameObject("neck_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_ARNeckPose = neckPoseObject.transform;

            m_NeckStartPose = new Pose(m_NeckBone.localPosition, m_NeckBone.localRotation);

            m_ARNeckPose.SetPositionAndRotation(m_NeckBone.position, Quaternion.identity);
            var neckOffset = new GameObject("neck_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            neckOffset.SetPositionAndRotation(m_NeckBone.position, m_NeckBone.rotation);
            neckOffset.SetParent(transform, true);
            m_ARNeckPose.SetParent(neckOffset, true);
            m_ARNeckPose.localRotation = Quaternion.identity;

            // Set Eye Look Rig
            var eyePoseObject = new GameObject("eye_pose"){ hideFlags = HideFlags.HideAndDontSave};
            m_AREyePose = eyePoseObject.transform;
            var eyePoseLookObject = new GameObject("eye_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyePoseLookAt = eyePoseLookObject.transform;
            m_EyePoseLookAt.SetParent(m_AREyePose);
            m_EyePoseLookAt.localPosition = Vector3.forward * -0.25f;

            // Eye Center Look
            m_AREyePose.SetPositionAndRotation(Vector3.Lerp(m_RightEye.position, m_LeftEye.position, 0.5f), transform.rotation);
            var eyeOffset = new GameObject("eye_offset"){ hideFlags = HideFlags.HideAndDontSave}.transform;
            eyeOffset.position = m_AREyePose.position;
            eyeOffset.SetParent(m_HeadBone, true);
            m_AREyePose.SetParent(eyeOffset, true);

            // Eye Right Look
            m_RightEyeStartPose = new Pose(m_RightEye.localPosition, m_RightEye.localRotation);
            var rightEyeOffset = m_RightEye.position - m_AREyePose.position;
            var eyeRightPoseLookObject = new GameObject("eye_right_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyeRightPoseLookAt = eyeRightPoseLookObject.transform;
            m_EyeRightPoseLookAt.SetParent(m_AREyePose);
            if (!m_RightEyeNegZ)
                m_EyeRightPoseLookAt.localPosition = Vector3.forward * -0.25f + rightEyeOffset;
            else
                m_EyeRightPoseLookAt.localPosition = Vector3.back * -0.25f + rightEyeOffset;

            // Eye Left Look
            m_LeftEyeStartPose = new Pose(m_LeftEye.localPosition, m_LeftEye.localRotation);
            var leftEyeOffset = m_LeftEye.position - m_AREyePose.position ;
            var eyeLeftPoseLookObject = new GameObject("eye_left_look"){ hideFlags = HideFlags.HideAndDontSave};
            m_EyeLeftPoseLookAt = eyeLeftPoseLookObject.transform;
            m_EyeLeftPoseLookAt.SetParent(m_AREyePose);
            if(!m_LeftEyeNegZ)
                m_EyeLeftPoseLookAt.localPosition = Vector3.forward * -0.25f + leftEyeOffset;
            else
                m_EyeLeftPoseLookAt.localPosition = Vector3.back * -0.25f + leftEyeOffset;

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

            animatedBones[0] = m_HeadBone;
            animatedBones[1] = m_NeckBone;
            animatedBones[2] = m_RightEye;
            animatedBones[3] = m_LeftEye;
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
            m_RightEye.localPosition = m_RightEyeStartPose.position;
            m_RightEye.localRotation = m_RightEyeStartPose.rotation;

            m_LeftEye.localPosition = m_LeftEyeStartPose.position;
            m_LeftEye.localRotation = m_LeftEyeStartPose.rotation;

            m_HeadBone.localPosition = m_HeadStartPose.position;
            m_HeadBone.localRotation = m_HeadStartPose.rotation;

            m_NeckBone.localPosition = m_NeckStartPose.position;
            m_NeckBone.localRotation = m_NeckStartPose.rotation;
        }

        void UpdateBoneTransforms()
        {
            if (m_RightEyeNegZ)
                m_RightEye.LookAt(m_EyeRightPoseLookAt, Vector3.down);
            else
                m_RightEye.LookAt(m_EyeRightPoseLookAt);

            if (m_LeftEyeNegZ)
                m_LeftEye.LookAt(m_EyeLeftPoseLookAt, Vector3.down);
            else
                m_LeftEye.LookAt(m_EyeLeftPoseLookAt);

            m_HeadBone.rotation = m_ARHeadPose.rotation;
            m_NeckBone.rotation = m_ARNeckPose.rotation;
        }

        void LateUpdate()
        {
            UpdateBoneTransforms();
        }
    }
}
