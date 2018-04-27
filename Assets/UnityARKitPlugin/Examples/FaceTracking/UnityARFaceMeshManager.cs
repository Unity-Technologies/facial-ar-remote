using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARFaceMeshManager : MonoBehaviour {

	[SerializeField]
	MeshFilter meshFilter;

	 UnityARSessionNativeInterface m_session;
	 Mesh faceMesh;

	// Use this for initialization
	void Start () {
		m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.enableLightEstimation = true;

		if (config.IsSupported && meshFilter != null) {

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
		UnityARFaceAnchorManager.Pose.position = position;
		UnityARFaceAnchorManager.Pose.rotation = rotation;
		gameObject.transform.localPosition = position;
		gameObject.transform.localRotation = rotation;
		UnityARFaceAnchorManager.active = true;

		faceMesh = new Mesh ();
		faceMesh.vertices = anchorData.faceGeometry.vertices;
		faceMesh.uv = anchorData.faceGeometry.textureCoordinates;
		faceMesh.triangles = anchorData.faceGeometry.triangleIndices;

		// Assign the mesh object and update it.
		faceMesh.RecalculateBounds();
		faceMesh.RecalculateNormals();
		meshFilter.mesh = faceMesh;
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{
		if (faceMesh != null) {
			var position = UnityARMatrixOps.GetPosition (anchorData.transform);
			var rotation = UnityARMatrixOps.GetRotation (anchorData.transform);
			UnityARFaceAnchorManager.Pose.position = position;
			UnityARFaceAnchorManager.Pose.rotation = rotation;
			gameObject.transform.localPosition = position;
			gameObject.transform.localRotation = rotation;
			faceMesh.vertices = anchorData.faceGeometry.vertices;
			//faceMesh.uv = anchorData.faceGeometry.textureCoordinates;
			//faceMesh.triangles = anchorData.faceGeometry.triangleIndices;
			//faceMesh.RecalculateBounds();
			faceMesh.RecalculateNormals();
		}
	}

	void FaceRemoved (ARFaceAnchor anchorData)
	{
		UnityARFaceAnchorManager.active = false;
		meshFilter.mesh = null;
		faceMesh = null;
	}
}
