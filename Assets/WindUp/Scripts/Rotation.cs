using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
	[SerializeField]
	float m_RotationAmount;
	
	void Update ()
	{
		var rot = m_RotationAmount * Time.deltaTime;
		transform.Rotate(Vector3.up, rot);
	}
}
