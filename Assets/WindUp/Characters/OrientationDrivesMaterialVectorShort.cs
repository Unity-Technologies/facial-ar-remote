using UnityEngine;

public class OrientationDrivesMaterialVectorShort : MonoBehaviour {

	public Transform trackThisOrientation;
	public Material materialToEdit;
	public Vector3 frontNormal;

	void Update () {
		frontNormal = (-trackThisOrientation.forward).normalized;
		materialToEdit.SetVector("_frontNormalWS", frontNormal);
	}

	void OnApplicationQuit(){
		frontNormal = new Vector3(0, 0, -1);
		materialToEdit.SetVector("_frontNormalWS", frontNormal);
	}
}
