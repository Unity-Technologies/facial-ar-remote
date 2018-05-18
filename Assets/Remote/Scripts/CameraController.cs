using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Labs.FacialRemote
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        Server m_Server;

        [Range(0.1f, 1)]
        [SerializeField]
        float m_CameraSmoothing = 0.8f;

        [SerializeField]
        Transform m_CameraTarget;

        void Start()
        {
            if (m_Server == null)
            {
                Debug.LogWarning("Camera Controller needs a Server set.");
                enabled = false;
            }
        }

        void Update()
        {
            if (!m_Server.running)
                return;

            var cameraRotation = m_Server.cameraPose.rotation;

            transform.localPosition = Vector3.Lerp(transform.localPosition, m_Server.cameraPose.position, m_CameraSmoothing);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, cameraRotation, m_CameraSmoothing);

            if (m_CameraTarget != null)
            {
                var toCamera = m_CameraTarget.position - m_Server.cameraPose.position;
                toCamera.y = 0;

//            if (toCamera.magnitude > 0)
//                m_HipsTransform.rotation = Quaternion.Lerp(m_HipsTransform.rotation, Quaternion.LookRotation(toCamera) * k_RotationOffset, m_FaceSmoothing);

            }

        }
    }
}
