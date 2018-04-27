using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARFaceAnchorManager : MonoBehaviour {

	[SerializeField]
	GameObject anchorPrefab;

	[Range(0.1f, 1f)]
	[SerializeField]
	float m_PositionSmoothing = 0.95f;

	[Range(0.1f, 1f)]
	[SerializeField]
	float m_RotationSmoothing = 0.8f;

	private UnityARSessionNativeInterface m_session;

	public static Pose Pose;
	public static bool active;

	// Use this for initialization
	void Start () {
		m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.enableLightEstimation = true;

		if (config.IsSupported ) {

			m_session.RunWithConfig (config);

			UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
			UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
			UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
		}

	}

	void FaceAdded (ARFaceAnchor anchorData)
	{
		var position = UnityARMatrixOps.GetPosition (anchorData.transform);
		var rotation = UnityARMatrixOps.GetRotation (anchorData.transform);
		Pose.position = position;
		Pose.rotation = rotation;
		anchorPrefab.transform.position = position;
		anchorPrefab.transform.rotation = rotation;
		active = true;
		anchorPrefab.SetActive (true);
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{
		var position = UnityARMatrixOps.GetPosition (anchorData.transform);
		var rotation = UnityARMatrixOps.GetRotation (anchorData.transform);
		Pose.position = position;
		Pose.rotation = rotation;
		anchorPrefab.transform.position = Vector3.Lerp(anchorPrefab.transform.position, position, m_PositionSmoothing);
		anchorPrefab.transform.rotation = Quaternion.Lerp(anchorPrefab.transform.rotation, rotation, m_RotationSmoothing);
	}

	void FaceRemoved (ARFaceAnchor anchorData)
	{
		active = false;
		anchorPrefab.SetActive (false);
	}
}
