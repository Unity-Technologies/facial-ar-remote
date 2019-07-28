using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    /// <inheritdoc cref="IUsesStreamReader" />
    /// <summary>
    /// Updates tracking pose values from the <see cref="StreamReader"/> to bone transforms for head, neck, and/or eyes. 
    /// </summary>
    /// TODO reset rig when drive changes
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CharacterRigController : MonoBehaviour, IUsesStreamReader
    {
        [SerializeField]
        [Tooltip("The camera being used to capture the character.")]
        Camera m_SceneCamera;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount to smoothing when returning to the start pose for the character when AR tracking is lost.")]
        float m_TrackingLossSmoothing = 0.1f;

        [SerializeField]
        [Tooltip("Enable controller driving eye bones pose.")]
        bool m_DriveEyes = true;

#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("Left eye bone transform")]
        Transform m_LeftEye;

        [SerializeField]
        [Tooltip("Right eye bone transform")]
        Transform m_RightEye;
#pragma warning restore CS0649
        
        [SerializeField]
        [Tooltip("Local offset distance for the eye look target")]
        float m_EyeLookDistance = -0.5f;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of smoothing to apply to eye movement")]
        float m_EyeSmoothing = 0.2f;

#pragma warning disable 649
        [SerializeField]
        [Tooltip("Corrects the right eye look direction if z is negative.")]
        bool m_RightEyeNegZ;

        [SerializeField]
        [Tooltip("Corrects the left eye look direction if z is negative.")]
        bool m_LeftEyeNegZ;
#pragma warning restore 649

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

#pragma warning disable CS0649
        [SerializeField]
        [Tooltip("Neck bone transform")]
        Transform m_NeckBone;
#pragma warning restore CS0649

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Amount of influence the AR head anchor pose has on the neck bone.")]
        float m_NeckFollowAmount = 0.4f;

        BlendShapeLocation m_EyeLookDownLeftLocation = BlendShapeLocation.EyeLookDownLeft;
        BlendShapeLocation m_EyeLookDownRightLocation = BlendShapeLocation.EyeLookDownRight;
        BlendShapeLocation m_EyeLookInLeftLocation = BlendShapeLocation.EyeLookInLeft;
        BlendShapeLocation m_EyeLookInRightLocation = BlendShapeLocation.EyeLookInRight;
        BlendShapeLocation m_EyeLookOutLeftLocation = BlendShapeLocation.EyeLookOutLeft;
        BlendShapeLocation m_EyeLookOutRightLocation = BlendShapeLocation.EyeLookOutRight;
        BlendShapeLocation m_EyeLookUpLeftLocation = BlendShapeLocation.EyeLookUpLeft;
        BlendShapeLocation m_EyeLookUpRightLocation = BlendShapeLocation.EyeLookUpRight;

        float m_EyeLookDownLeft;
        float m_EyeLookDownRight;
        float m_EyeLookInLeft;
        float m_EyeLookInRight;
        float m_EyeLookOutLeft;
        float m_EyeLookOutRight;
        float m_EyeLookUpLeft;
        float m_EyeLookUpRight;
        
        [SerializeField, HideInInspector]
        Transform m_HeadOffsetObject;
        [SerializeField, HideInInspector]
        GameObject m_NeckPoseObject;
        [SerializeField, HideInInspector]
        Transform m_NeckOffsetObject;
        [SerializeField, HideInInspector]
        GameObject m_LocHeadOffsetObject;
        [SerializeField, HideInInspector]
        Transform m_LocalizedHeadRotObject;
        [SerializeField, HideInInspector]
        Transform m_OtherThingOffsetObject;
        [SerializeField, HideInInspector]
        GameObject m_EyeLeftPoseLookObject;
        [SerializeField, HideInInspector]
        GameObject m_EyeRightPoseLookObject;
        [SerializeField, HideInInspector]
        Transform m_EyeOffsetObject;
        [SerializeField, HideInInspector]
        GameObject m_EyePoseLookObject;
        [SerializeField, HideInInspector]
        GameObject m_EyePoseObject;
        [SerializeField, HideInInspector]
        Transform m_ARHeadPoseObject;
        [SerializeField, HideInInspector]
        Transform m_ARNeckPose;
        [SerializeField, HideInInspector]
        Transform m_AREyePose;
        [SerializeField, HideInInspector]
        Transform m_EyePoseLookAt;
        [SerializeField, HideInInspector]
        Transform m_EyeRightPoseLookAt;
        [SerializeField, HideInInspector]
        Transform m_EyeLeftPoseLookAt;
        [SerializeField, HideInInspector]
        Transform m_LocalizedHeadParent;
        [SerializeField, HideInInspector]
        Transform m_LocalizedHeadRot;
        [SerializeField, HideInInspector]
        Transform m_OtherThing;
        [SerializeField, HideInInspector]
        Transform m_OtherLookObject;

        Pose m_HeadStartPose;
        Pose m_NeckStartPose;
        Pose m_RightEyeStartPose;
        Pose m_LeftEyeStartPose;

        Quaternion m_LastHeadRotation;
        Quaternion m_LastNeckRotation;

        [SerializeField, HideInInspector]
        bool m_IsRigSetup = false;
        
        IStreamSettings m_LastStreamSettings;

        public IStreamReader streamReader { private get; set; }

        public Transform headBone
        {
            get
            {
                return m_HeadBone != null ? m_HeadBone : transform;
            }
            private set
            {
                if (m_HeadBone == value)
                    return;

                m_HeadBone = value;
                SetupCharacterRigController();
            }
        }

        public bool driveEyes
        {
            get { return m_DriveEyes; }
            set { m_DriveEyes = value; }
        }

        public bool driveHead
        {
            get { return m_DriveHead; }
            set { m_DriveHead = value; }
        }

        public bool driveNeck
        {
            get { return m_DriveNeck; }
            set { m_DriveNeck = value; }
        }

        public float neckFollowAmount
        {
            get { return m_NeckFollowAmount; }
            set { m_NeckFollowAmount = value; }
        }

        public float headSmoothing
        {
            get { return m_HeadSmoothing; }
            set { m_HeadSmoothing = value; }
        }

        public Vector2 eyeAngleRange
        {
            get { return m_EyeAngleRange; }
            set { m_EyeAngleRange = value; }
        }

        public Camera sceneCamera
        {
            get { return m_SceneCamera; }
            set
            {
                if (m_SceneCamera == value)
                    return;

                m_SceneCamera = value;
                
                if (m_SceneCamera != null)
                    streamReader.SetInitialCameraPose(new Pose(sceneCamera.transform.position, sceneCamera.transform.rotation));
            }
        }

        [NonSerialized]
        public Transform[] animatedBones = new Transform [4];

        void LateUpdate()
        {
            var streamSource = streamReader?.streamSource;
            if (streamSource == null || !streamSource.isActive)
                return;

            if (!m_IsRigSetup)
                return;

            GenerateRigRotationsFromBlendShapes();
            UpdateBoneTransforms();
        }

        /// <summary>
        /// Create transform references and helper game objects for driving the values of the face rig. This must be
        /// called each time the rig changes
        /// </summary>
        public void SetupCharacterRigController()
        {
            Debug.Log("Start avatar Setup");

            if (driveEyes)
            {
                if (m_LeftEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Left Eye Bone is returning null. Disabling Drive Eyes.");
                    driveEyes = false;
                }

                if (m_RightEye == null)
                {
                    Debug.LogWarning("Drive Eyes is set but Right Eye Bone is returning null. Disabling Drive Eyes.");
                    driveEyes = false;
                }
            }

            if (driveHead && m_HeadBone == null)
            {
                Debug.LogWarning("Drive Head is set but Head Bone is returning null. Disabling Drive Head.");
                driveHead = false;
            }

            if (driveNeck && m_NeckBone == null)
            {
                Debug.LogWarning("Drive Neck is set but Neck Bone is returning null. Disabling Drive Neck.");
                driveNeck = false;
            }

            Pose eyeLeftWorldPose;
            Pose eyeLeftLocalPose;
            Pose eyeRightWorldPose;
            Pose eyeRightLocalPose;
            if (driveEyes)
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
                var headWorldPose = GetHeadPose(Space.World);
                var headLocalPose = GetHeadPose(Space.Self);
                eyeLeftWorldPose = headWorldPose;
                eyeLeftLocalPose = headLocalPose;
                eyeRightWorldPose = headWorldPose;
                eyeRightLocalPose = headLocalPose;
            }

            // Set Head Look Rig
            if (m_ARHeadPoseObject == null)
                m_ARHeadPoseObject = new GameObject("head_pose").transform; //{ hideFlags = HideFlags.HideAndDontSave};

            m_HeadStartPose = GetHeadPose(Space.Self);

            m_ARHeadPoseObject.SetPositionAndRotation(GetHeadPose(Space.World).position, Quaternion.identity);
            
            if (m_HeadOffsetObject == null)
                m_HeadOffsetObject = new GameObject("head_offset").transform;
            m_HeadOffsetObject.SetPositionAndRotation(GetHeadPose(Space.World).position, GetHeadPose(Space.World).rotation);
            m_HeadOffsetObject.SetParent(transform, true);
            m_ARHeadPoseObject.SetParent(m_HeadOffsetObject, true);
            m_ARHeadPoseObject.localRotation = Quaternion.identity;

            // Set Neck Look Rig
            if (m_NeckPoseObject == null)
                m_NeckPoseObject = new GameObject("neck_pose");
            m_ARNeckPose = m_NeckPoseObject.transform;

            m_NeckStartPose = GetNeckPose(Space.Self);

            m_ARNeckPose.SetPositionAndRotation(GetNeckPose(Space.World).position, Quaternion.identity);
            if (m_NeckOffsetObject == null)
                m_NeckOffsetObject = new GameObject("neck_offset").transform;
            m_NeckOffsetObject.SetPositionAndRotation(GetNeckPose(Space.World).position, GetNeckPose(Space.World).rotation);
            m_NeckOffsetObject.SetParent(transform, true);
            m_ARNeckPose.SetParent(m_NeckOffsetObject, true);
            m_ARNeckPose.localRotation = Quaternion.identity;

            // Set Eye Look Rig
            if (m_EyePoseObject == null)
                m_EyePoseObject = new GameObject("eye_pose");
            m_AREyePose = m_EyePoseObject.transform;
            if (m_EyePoseLookObject == null)
                m_EyePoseLookObject = new GameObject("eye_look");
            m_EyePoseLookAt = m_EyePoseLookObject.transform;
            m_EyePoseLookAt.SetParent(m_AREyePose);
            m_EyePoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance;

            // Eye Center Look
            // TODO should the rotation be transform.rotation or the head transform?
            m_AREyePose.SetPositionAndRotation(Vector3.Lerp(eyeRightWorldPose.position, eyeLeftWorldPose.position, 0.5f), transform.rotation);
            if (m_EyeOffsetObject == null)
                m_EyeOffsetObject = new GameObject("eye_offset").transform;
            m_EyeOffsetObject.position = m_AREyePose.position;
            m_EyeOffsetObject.SetParent(headBone, true);

            m_AREyePose.SetParent(m_EyeOffsetObject, true);

            // Eye Right Look
            m_RightEyeStartPose = new Pose(eyeRightLocalPose.position, eyeRightLocalPose.rotation);
            var rightEyeOffset = eyeRightWorldPose.position - m_AREyePose.position;
            if (m_EyeRightPoseLookObject == null)
                m_EyeRightPoseLookObject = new GameObject("eye_right_look");
            m_EyeRightPoseLookAt = m_EyeRightPoseLookObject.transform;
            m_EyeRightPoseLookAt.SetParent(m_AREyePose);
            if (!m_RightEyeNegZ)
                m_EyeRightPoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance + rightEyeOffset;
            else
                m_EyeRightPoseLookAt.localPosition = Vector3.back * m_EyeLookDistance + rightEyeOffset;

            // Eye Left Look
            m_LeftEyeStartPose = new Pose(eyeLeftLocalPose.position, eyeLeftLocalPose.rotation);
            var leftEyeOffset = eyeLeftWorldPose.position - m_AREyePose.position;
            if (m_EyeLeftPoseLookObject == null)
                m_EyeLeftPoseLookObject = new GameObject("eye_left_look");
            m_EyeLeftPoseLookAt = m_EyeLeftPoseLookObject.transform;
            m_EyeLeftPoseLookAt.SetParent(m_AREyePose);
            if (!m_LeftEyeNegZ)
                m_EyeLeftPoseLookAt.localPosition = Vector3.forward * m_EyeLookDistance + leftEyeOffset;
            else
                m_EyeLeftPoseLookAt.localPosition = Vector3.back * m_EyeLookDistance + leftEyeOffset;

            m_AREyePose.rotation = Quaternion.identity;

            // Other strange rig stuff
            if (m_LocalizedHeadParent == null)
                m_LocalizedHeadParent = new GameObject("loc_head_parent").transform; //{ hideFlags = HideFlags.HideAndDontSave}.transform;
            if (m_LocHeadOffsetObject == null)
                m_LocHeadOffsetObject = new GameObject("loc_head_offset");
            m_LocHeadOffsetObject.transform.SetParent(m_LocalizedHeadParent);

            if (m_LocalizedHeadRotObject == null)
                m_LocalizedHeadRotObject = m_LocalizedHeadRot = new GameObject("loc_head_rot").transform;
            m_LocalizedHeadRot.SetParent(m_LocHeadOffsetObject.transform);
            if (m_OtherLookObject == null)
                m_OtherLookObject = new GameObject("other_look").transform; //{ hideFlags = HideFlags.HideAndDontSave}.transform;
            m_OtherLookObject.SetParent(m_LocalizedHeadRot.transform);
            m_OtherLookObject.localPosition = Vector3.forward * 0.25f;

            if (m_OtherThingOffsetObject == null)
                m_OtherThingOffsetObject = new GameObject("other_thing_offset").transform;
            m_OtherThingOffsetObject.transform.SetParent(m_LocalizedHeadParent);
            m_OtherThingOffsetObject.transform.rotation = transform.rotation;
            if (m_OtherThing == null)
                m_OtherThing = new GameObject("other_thing").transform; //{ hideFlags = HideFlags.HideAndDontSave}.transform;
            m_OtherThing.SetParent(m_OtherThingOffsetObject.transform);

            if (m_HeadBone != null)
                animatedBones[0] = m_HeadBone;

            if (m_NeckBone != null)
                animatedBones[1] = m_NeckBone;

            if (m_LeftEye != null)
                animatedBones[2] = m_LeftEye;

            if (m_RightEye != null)
                animatedBones[3] = m_RightEye;
        }

        Pose GetHeadPose(Space space)
        {
            if (space == Space.Self)
            {
                if (driveHead)
                    return new Pose(headBone.localPosition, headBone.localRotation);
                
                if (driveNeck)
                    return new Pose(m_NeckBone.localPosition, m_NeckBone.localRotation);
                
                return new Pose(transform.localPosition, transform.localRotation);
            }
            else
            {
                if (driveHead)
                    return new Pose(headBone.position, headBone.rotation);
                
                if (driveNeck)
                    return new Pose(m_NeckBone.position, m_NeckBone.rotation);
                
                return new Pose(transform.position, transform.rotation);
            }
        }

        Pose GetNeckPose(Space space)
        {
            if (space == Space.Self)
            {
                if (driveNeck)
                    return new Pose(m_NeckBone.localPosition, m_NeckBone.localRotation);
                
                return new Pose(headBone.localPosition, headBone.localRotation);
            }
            else
            {
                if (driveNeck)
                    return new Pose(m_NeckBone.position, m_NeckBone.rotation);
                
                return new Pose(headBone.position, headBone.rotation);
            }
        }

        /// <summary>
        /// Reset all bone poses to their starting positions.
        /// </summary>
        public void ResetBonePoses()
        {
            if (driveEyes)
            {
                m_RightEye.localPosition = m_RightEyeStartPose.position;
                m_RightEye.localRotation = m_RightEyeStartPose.rotation;

                m_LeftEye.localPosition = m_LeftEyeStartPose.position;
                m_LeftEye.localRotation = m_LeftEyeStartPose.rotation;
            }

            if (driveHead)
            {
                m_HeadBone.localPosition = m_HeadStartPose.position;
                m_HeadBone.localRotation = m_HeadStartPose.rotation;
            }

            if (driveNeck)
            {
                m_NeckBone.localPosition = m_NeckStartPose.position;
                m_NeckBone.localRotation = m_NeckStartPose.rotation;
            }
        }

        /// <summary>
        /// Generate a new eye poses and head and neck rotations based on blend shape values.
        /// </summary>
        /// <param name="force">Force rig movement even if tracking is lost.</param>
        public void GenerateRigRotationsFromBlendShapes(bool force = false)
        {
            if (force || !streamReader.faceTrackingLost)
            {
                LocalizeFacePose();

                var buffer = streamReader.blendShapesBuffer;
                
                // Interpolate blend shape values driving eye movement
                m_EyeLookDownLeft = Mathf.Lerp(buffer[(int)m_EyeLookDownLeftLocation], m_EyeLookDownLeft, m_EyeSmoothing);
                m_EyeLookInLeft = Mathf.Lerp(buffer[(int)m_EyeLookInLeftLocation], m_EyeLookInLeft, m_EyeSmoothing);
                m_EyeLookOutLeft = Mathf.Lerp(buffer[(int)m_EyeLookOutLeftLocation], m_EyeLookOutLeft, m_EyeSmoothing);
                m_EyeLookUpLeft = Mathf.Lerp(buffer[(int)m_EyeLookUpLeftLocation], m_EyeLookUpLeft, m_EyeSmoothing);

                m_EyeLookDownRight = Mathf.Lerp(buffer[(int)m_EyeLookDownRightLocation], m_EyeLookDownRight, m_EyeSmoothing);
                m_EyeLookInRight = Mathf.Lerp(buffer[(int)m_EyeLookInRightLocation], m_EyeLookInRight, m_EyeSmoothing);
                m_EyeLookOutRight = Mathf.Lerp(buffer[(int)m_EyeLookOutRightLocation], m_EyeLookOutRight, m_EyeSmoothing);
                m_EyeLookUpRight = Mathf.Lerp(buffer[(int)m_EyeLookUpRightLocation], m_EyeLookUpRight, m_EyeSmoothing);

                // Calculate new eye rotations
                var leftEyePitch = Quaternion.AngleAxis((m_EyeLookUpLeft - m_EyeLookDownLeft) * eyeAngleRange.x, Vector3.right);
                var leftEyeYaw = Quaternion.AngleAxis((m_EyeLookInLeft - m_EyeLookOutLeft) * eyeAngleRange.y, Vector3.up);
                var leftEyeRot = leftEyePitch * leftEyeYaw;

                var rightEyePitch = Quaternion.AngleAxis((m_EyeLookUpRight - m_EyeLookDownRight) * eyeAngleRange.x, Vector3.right);
                var rightEyeYaw = Quaternion.AngleAxis((m_EyeLookOutRight - m_EyeLookInRight) * eyeAngleRange.y, Vector3.up);
                var rightEyeRot = rightEyePitch * rightEyeYaw;

                // Set new look-at transform
                m_AREyePose.localRotation = Quaternion.Slerp(leftEyeRot, rightEyeRot, 0.5f);

                // Rotate the head
                var headRot = m_LocalizedHeadRot.localRotation;
                headRot = Quaternion.Slerp(m_HeadStartPose.rotation, headRot, m_HeadFollowAmount);
                m_ARHeadPoseObject.localRotation = Quaternion.Slerp(headRot, m_LastHeadRotation, headSmoothing);
                m_LastHeadRotation = m_ARHeadPoseObject.localRotation;

                // Rotate the neck
                var neckRot = headRot;
                neckRot = Quaternion.Slerp(m_NeckStartPose.rotation, neckRot, neckFollowAmount);
                m_ARNeckPose.localRotation = Quaternion.Slerp(neckRot, m_LastNeckRotation, headSmoothing);
                m_LastNeckRotation = m_ARNeckPose.localRotation;
            }
            else
            {
                if (driveEyes)
                {
                    // TODO should this drive from the last position instead of an identity Quaternion?
                    m_AREyePose.localRotation = Quaternion.Slerp(Quaternion.identity, m_AREyePose.localRotation, m_TrackingLossSmoothing);
                    m_ARHeadPoseObject.localRotation = Quaternion.Slerp(Quaternion.identity, m_ARHeadPoseObject.localRotation, m_TrackingLossSmoothing);
                }

                m_LastHeadRotation = m_ARHeadPoseObject.localRotation;
            }

            if (force)
                UpdateBoneTransforms();
        }

        /// <summary>
        /// Localizes the head pose to rotation between the real face and the device camera
        /// </summary>
        void LocalizeFacePose()
        {
            if (streamReader == null)
                return;
            
            var headPose = streamReader.headPose;
            m_LocalizedHeadParent.position = headPose.position;
            m_LocalizedHeadParent.LookAt(streamReader.cameraPose.position);

            m_LocalizedHeadRot.rotation = headPose.rotation * Quaternion.Euler(0, 180, 0);;
            m_OtherThing.LookAt(m_OtherLookObject, m_OtherLookObject.up);
        }

        void UpdateBoneTransforms()
        {
            if (driveEyes)
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

            if (driveHead)
                headBone.rotation = m_ARHeadPoseObject.rotation;

            if (driveNeck)
                m_NeckBone.rotation = m_ARNeckPose.rotation;
        }
        
        public 

        void OnValidate()
        {
            if (sceneCamera == null)
            {
                m_SceneCamera = Camera.main;
            }
        }
    }
}
