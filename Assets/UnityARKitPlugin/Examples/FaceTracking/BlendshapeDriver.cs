using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BlendshapeDriver : MonoBehaviour
{
	[SerializeField]
	string m_BlendshapePrefix = "blendShape1.";

	SkinnedMeshRenderer skinnedMeshRenderer;
	Dictionary<string, float> currentBlendShapes;
	Dictionary<string, int> blendShapeIndices;

	// Use this for initialization
	void Start () {
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();

		if (skinnedMeshRenderer) {
			UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
			UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
		}
	}

	void FaceAdded (ARFaceAnchor anchorData)
	{
		currentBlendShapes = anchorData.blendShapes;

		if (blendShapeIndices == null)
		{
			blendShapeIndices = new Dictionary<string, int>();

			var skinnedMesh = skinnedMeshRenderer.sharedMesh;
			foreach (var kvp in currentBlendShapes)
			{
				blendShapeIndices[kvp.Key] = skinnedMesh.GetBlendShapeIndex(string.Format("{0}{1}", m_BlendshapePrefix, kvp.Key));
			}
		}
	}

	void FaceUpdated (ARFaceAnchor anchorData)
	{
		currentBlendShapes = anchorData.blendShapes;
	}

	void Update () {
		if (currentBlendShapes != null)
		{
			foreach(var kvp in currentBlendShapes)
			{
				var blendShapeIndex = blendShapeIndices[kvp.Key];
				if (blendShapeIndex >= 0 ) {
					skinnedMeshRenderer.SetBlendShapeWeight (blendShapeIndex, kvp.Value * 100.0f);
				}
			}
		}
	}
}
