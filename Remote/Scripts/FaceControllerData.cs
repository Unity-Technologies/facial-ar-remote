using System;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [Serializable]
    public class FaceControllerData
    {
        [SerializeField]
        [Tooltip("Root of character to be be driven.")]
        GameObject m_CharacterGameObject;

        [SerializeField]
        [Tooltip("(Optional) Manually override the blend shape controller found in the Character.")]
        BlendShapesController m_BlendShapesControllerOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the character rig controller found in the Character.")]
        CharacterRigController m_CharacterRigControllerOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the head bone set from the character rig controller. Used for determining the start pose of the character.")]
        Transform m_HeadBoneOverride;

        [SerializeField]
        [Tooltip("(Optional) Manually override the main camera found by the stream reader. Used for determining the starting pose of the camera.")]
        Camera m_CameraOverride;

        public GameObject characterGameObject
        {
            get { return m_CharacterGameObject; }
            set { m_CharacterGameObject = value; }
        }

        public BlendShapesController blendShapesControllerOverride
        {
            get { return m_BlendShapesControllerOverride; }
            set { m_BlendShapesControllerOverride = value; }
        }

        public CharacterRigController characterRigControllerOverride
        {
            get { return m_CharacterRigControllerOverride; }
            set { m_CharacterRigControllerOverride = value; }
        }

        public Transform headBoneOverride
        {
            get { return m_HeadBoneOverride; }
            set { m_HeadBoneOverride = value; }
        }

        public Camera cameraOverride
        {
            get { return m_CameraOverride; }
            set { m_CameraOverride = value; }
        }
    }
}
