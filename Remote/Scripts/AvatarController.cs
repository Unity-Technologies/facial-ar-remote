using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Unity.Labs.FacialRemote
{
    public class AvatarController : MonoBehaviour
    {
        [SerializeField]
        BlendShapeReader m_Reader;

        [SerializeField]
        Animator m_Animator;

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
        float m_Weight = 1;

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

        Transform m_HeadBone;
        Transform m_ARHeadPose;
        Transform m_AREyePose;

        [SerializeField]
        Transform m_HeadPoseLookAt;
        [SerializeField]
        Transform m_EyePoseLookAt;

        RuntimeAnimatorController m_AnimatorController;
        int m_HeadLookLayer = -1;
        int m_EyeLookLayer = -1;

        Coroutine m_AnimatorSetup;

        Quaternion m_LastHeadRotation;

        [SerializeField]
        bool m_RotNeck;

        [SerializeField]
        bool m_RotHead;

        [SerializeField]
        [Range(0,1)]
        float m_NeckAmount;

        [SerializeField]
        [Range(0,1)]
        float m_HeadAmount;

        Transform m_LocalizedHeadParent;
        Transform m_LocalizedHeadRot;
        Transform m_OtherThing;
        Transform m_OtherLook;

        [SerializeField]
        bool m_UseLocalizedHeadRot;

        bool animatorReady
        {
            get
            {
                return m_AnimatorController != null && m_AREyePose != null
                    && m_LeftEye != null && m_RightEye != null;
            }
        }

        void Start()
        {
            if (m_Reader == null)
            {
                Debug.LogWarning("Avatar Controller needs a Server set.");
                enabled = false;
                return;
            }

            if (m_Animator == null)
            {
                Debug.LogWarning("Avatar Controller needs an Animator set.");
                enabled = false;
                return;
            }

            m_EyeLookDownLeftIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookDownLeft);
            m_EyeLookDownRightIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookDownRight);
            m_EyeLookInLeftIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookInLeft);
            m_EyeLookInRightIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookInRight);
            m_EyeLookOutLeftIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookOutLeft);
            m_EyeLookOutRightIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookOutRight);
            m_EyeLookUpLeftIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookUpLeft);
            m_EyeLookUpRightIndex = m_Reader.streamSettings.GetLocationIndex(ARBlendShapeLocation.EyeLookUpRight);

//            m_AnimatorSetup = StartCoroutine(SetupAnimator());
        }

        void OnEnable()
        {
            if (m_AnimatorSetup == null)
                m_AnimatorSetup = StartCoroutine(SetupAnimator());
        }

        void OnDisable()
        {
            if (m_AnimatorController != null)
            {
#if UNITY_EDITOR
                var controller = m_AnimatorController as AnimatorController;
                if (controller == null)
                    return;

                if (m_EyeLookLayer != -1)
                {
                    controller.RemoveLayer(m_EyeLookLayer);
                    m_EyeLookLayer = -1;
                }

                if (m_HeadLookLayer != -1)
                {
                    controller.RemoveLayer(m_HeadLookLayer);
                    m_HeadLookLayer = -1;
                }
#endif
            }
        }

        IEnumerator SetupAnimator()
        {
            while (m_Animator.isInitialized)
            {
                if (m_AnimatorController == null)
                {
                    m_AnimatorController = m_Animator.runtimeAnimatorController;
                    if (m_AnimatorController == null)
                    {
                        enabled = false;

                        StopCoroutine(m_AnimatorSetup);
                        m_AnimatorSetup = null;
                        yield return null;
                    }

                    yield return null;
                }

#if UNITY_EDITOR
                if (m_HeadLookLayer == -1 || m_EyeLookLayer == -1)
                {
                    var controller = m_AnimatorController as AnimatorController;
                    if (controller == null)
                    {
                        m_HeadLookLayer = 0;
                        m_EyeLookLayer = 0;
                        yield return null;
                    }

                    if (m_HeadLookLayer == -1)
                    {
                        var headLookLayer = new AnimatorControllerLayer
                        {
                            blendingMode = AnimatorLayerBlendingMode.Override,
                            defaultWeight = 0f,
                            iKPass = true,
                            name = controller.MakeUniqueLayerName("HeadLookLayer"),
                        };
                        controller.AddLayer(headLookLayer);
                        m_HeadLookLayer = controller.layers.Length - 1;
                    }

                    if (m_EyeLookLayer == -1)
                    {
                        var eyeLookLayer = new AnimatorControllerLayer
                        {
                            blendingMode = AnimatorLayerBlendingMode.Additive,
                            defaultWeight = 0f,
                            iKPass = true,
                            name = controller.MakeUniqueLayerName("EyeLookLayer")
                        };
                        controller.AddLayer(eyeLookLayer);
                        m_EyeLookLayer = controller.layers.Length - 1;
                    }

                    yield return null;
                }
#endif

                if (m_LeftEye == null || m_RightEye == null)
                {
                    if(m_LeftEye == null)
                        m_LeftEye = m_Animator.GetBoneTransform(HumanBodyBones.LeftEye);

                    if (m_RightEye == null)
                        m_RightEye = m_Animator.GetBoneTransform(HumanBodyBones.RightEye);

                    yield return null;
                }

                if (m_ARHeadPose == null)
                {
                    m_HeadBone = m_Animator.GetBoneTransform(HumanBodyBones.Head);
                    if (m_HeadBone == null)
                    {
                        Debug.LogError("Head Bone returning NULL!");
                        enabled = false;

                        StopCoroutine(m_AnimatorSetup);
                        m_AnimatorSetup = null;
                        yield return null;
                    }

                    var headPoseObject = new GameObject("head_pose");
                    m_ARHeadPose = headPoseObject.transform;
                    var headPoseLookObject = new GameObject("head_look");
                    m_HeadPoseLookAt = headPoseLookObject.transform;

                    m_ARHeadPose.SetPositionAndRotation(m_HeadBone.position, Quaternion.identity);
                    var headOffset = new GameObject("head_offset").transform;
                    headOffset.SetPositionAndRotation(m_HeadBone.position, Quaternion.identity);
                    headOffset.SetParent(transform, true);
                    m_ARHeadPose.SetParent(headOffset, true);
                    m_HeadPoseLookAt.SetParent(m_ARHeadPose);
                    m_HeadPoseLookAt.localPosition = Vector3.forward * -0.25f;

                    var eyePoseObject = new GameObject("eye_pose");
                    m_AREyePose = eyePoseObject.transform;
                    var eyePoseLookObject = new GameObject("eye_look");
                    m_EyePoseLookAt = eyePoseLookObject.transform;
                    m_EyePoseLookAt.SetParent(m_AREyePose);
                    m_EyePoseLookAt.localPosition = Vector3.forward * -0.25f;

                    m_AREyePose.SetPositionAndRotation(Vector3.Lerp(m_RightEye.position, m_LeftEye.position, 0.5f), transform.rotation);
                    var eyeOffset = new GameObject("eye_offset").transform;
                    eyeOffset.position = m_AREyePose.position;
                    eyeOffset.SetParent(m_HeadBone, true);
                    m_AREyePose.SetParent(eyeOffset, true);

                    m_LocalizedHeadParent = new GameObject("loc_head_parent").transform;
                    var locHeadOffset = new GameObject("loc_head_offset");
                    locHeadOffset.transform.SetParent(m_LocalizedHeadParent);
                    m_LocalizedHeadRot = new GameObject("loc_head_rot").transform;
                    m_LocalizedHeadRot.SetParent(locHeadOffset.transform);
                    m_OtherLook = new GameObject("other_look").transform;
                    m_OtherLook.SetParent(m_LocalizedHeadRot.transform);
                    m_OtherLook.localPosition = Vector3.forward * 0.25f;

                    var otherThingOffset = new GameObject("other_thing_offset");
                    otherThingOffset.transform.SetParent(m_LocalizedHeadParent);
                    otherThingOffset.transform.rotation = transform.rotation;
                    m_OtherThing = new GameObject("other_thing").transform;
                    m_OtherThing.SetParent(otherThingOffset.transform);

                    yield return null;
                }

                yield return null;
            }

            yield return null;
        }

        void LocalizeFacePose()
        {
            var mirror = m_Reader.headPose.rotation;
            mirror.w *= -1f;

            m_LocalizedHeadParent.position = m_Reader.headPose.position;
            m_LocalizedHeadParent.LookAt(m_Reader.cameraPose.position);

            m_LocalizedHeadRot.rotation = m_Reader.headPose.rotation;
            m_OtherThing.LookAt(m_OtherLook, m_OtherLook.up);
        }

        void Update()
        {
            if (!m_Reader.streamActive || !animatorReady)
                return;

            if (m_AnimatorSetup != null)
            {
                StopCoroutine(m_AnimatorSetup);
                m_AnimatorSetup = null;
            }

            if (m_Reader.faceActive)
            {
                LocalizeFacePose();

                m_EyeLookDownLeft = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookDownLeftIndex], m_EyeLookDownLeft, m_EyeSmoothing);
                m_EyeLookInLeft = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookInLeftIndex], m_EyeLookInLeft, m_EyeSmoothing);
                m_EyeLookOutLeft = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookOutLeftIndex], m_EyeLookOutLeft, m_EyeSmoothing);
                m_EyeLookUpLeft = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookUpLeftIndex], m_EyeLookUpLeft, m_EyeSmoothing);

                m_EyeLookDownRight = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookDownRightIndex], m_EyeLookDownRight, m_EyeSmoothing);
                m_EyeLookInRight = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookInRightIndex], m_EyeLookInRight, m_EyeSmoothing);
                m_EyeLookOutRight = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookOutRightIndex], m_EyeLookOutRight, m_EyeSmoothing);
                m_EyeLookUpRight = Mathf.Lerp(m_Reader.blendShapesBuffer[m_EyeLookUpRightIndex], m_EyeLookUpRight, m_EyeSmoothing);

//                var leftEyePitch = Quaternion.AngleAxis((m_EyeLookDownLeft - m_EyeLookUpLeft) * m_EyeAngleRange.x, Vector3.right);
                var leftEyePitch = Quaternion.AngleAxis((m_EyeLookUpLeft - m_EyeLookDownLeft) * m_EyeAngleRange.x, Vector3.right);
                var leftEyeYaw = Quaternion.AngleAxis((m_EyeLookInLeft - m_EyeLookOutLeft) * m_EyeAngleRange.y, Vector3.up);
                var leftEyeRot = leftEyePitch * leftEyeYaw;

//                var rightEyePitch = Quaternion.AngleAxis((m_EyeLookDownRight - m_EyeLookUpRight) * m_EyeAngleRange.x, Vector3.right);
                var rightEyePitch = Quaternion.AngleAxis((m_EyeLookUpRight - m_EyeLookDownRight) * m_EyeAngleRange.x, Vector3.right);
                var rightEyeYaw = Quaternion.AngleAxis((m_EyeLookOutRight - m_EyeLookInRight) * m_EyeAngleRange.y, Vector3.up);
                var rightEyeRot = rightEyePitch * rightEyeYaw;

                m_AREyePose.localRotation = Quaternion.Slerp(leftEyeRot, rightEyeRot, 0.5f);

                var headRot = m_UseLocalizedHeadRot ? m_OtherThing.localRotation : m_Reader.headPose.rotation;
                m_ARHeadPose.localRotation = Quaternion.Slerp(headRot, m_LastHeadRotation, m_HeadSmoothing);
                m_LastHeadRotation = m_ARHeadPose.localRotation;
            }
            else
            {
                m_AREyePose.localRotation = Quaternion.Slerp(Quaternion.identity, m_AREyePose.localRotation, m_TrackingLossSmoothing);
                m_ARHeadPose.localRotation = Quaternion.Slerp(Quaternion.identity, m_ARHeadPose.localRotation, m_TrackingLossSmoothing);
                m_LastHeadRotation = m_ARHeadPose.localRotation;
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        float m_TrackingLossSmoothing = 0.1f;

        void OnAnimatorIK(int layerIndex)
        {
            if (!animatorReady)
                return;

#if UNITY_EDITOR
            var controller = m_AnimatorController as AnimatorController;
            if (controller == null)
                return;

            if (layerIndex == m_HeadLookLayer)
            {
                controller.layers[layerIndex].defaultWeight = 1f;
                m_Animator.SetLookAtWeight(1f, 0, m_Weight, 0f);
                var headLookPos = m_HeadPoseLookAt.position - m_HeadBone.position + m_Animator.transform.position;
                m_Animator.SetLookAtPosition(headLookPos);

                var mirror =  m_OtherThing.localRotation;
                mirror.w *= -1f;

                //TODO hacky
                if (m_RotNeck)
                {
                    var neckRot = m_Reader.trackingActive ?
                        Quaternion.Slerp(m_Animator.GetBoneTransform(HumanBodyBones.Neck).localRotation, mirror, m_NeckAmount) :
                        Quaternion.Slerp(Quaternion.identity, m_Animator.GetBoneTransform(HumanBodyBones.Neck).localRotation, m_TrackingLossSmoothing);
                    m_Animator.SetBoneLocalRotation(HumanBodyBones.Neck, neckRot);
                }

                if(m_RotHead)
                {
                    var headRot = m_Reader.trackingActive ?
                        Quaternion.Slerp(m_Animator.GetBoneTransform(HumanBodyBones.Head).localRotation, mirror, m_HeadAmount) :
                        Quaternion.Slerp(Quaternion.identity, m_Animator.GetBoneTransform(HumanBodyBones.Head).localRotation, m_HeadAmount);
                    m_Animator.SetBoneLocalRotation(HumanBodyBones.Head, headRot);
                }
            }
            else if (layerIndex == m_EyeLookLayer)
            {
                controller.layers[layerIndex].defaultWeight = 1f;
                m_Animator.SetLookAtWeight(1f,0f,0f,1f);
                var eyeLookPos = m_EyePoseLookAt.position - m_AREyePose.position + m_Animator.transform.position;
                m_Animator.SetLookAtPosition(eyeLookPos);
            }
#endif
        }
    }
}
