using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSelector : MonoBehaviour
{
    [SerializeField]
    GameObject[] m_Faces;

    [SerializeField]
    int m_ButtonWidth = 200;

    [SerializeField]
    int m_ButtonHeight = 60;

    [SerializeField]
    int m_Padding = 5;

	void Start ()
    {
        foreach (var face in m_Faces)
        {
            if (face != null)
            {
                SetActiveFace(face);
                break;
            }
        }
	}

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        var rect = new Rect(w - m_ButtonWidth - m_Padding, 0, m_Padding, m_Padding);

        foreach (GameObject face in m_Faces)
        {
            if (face == null)
                continue;
            rect = new Rect(rect.xMin, rect.yMax, m_ButtonWidth, m_ButtonHeight);
            if (GUI.Button(rect, face.name))
            {
                SetActiveFace(face);
            }
            rect = new Rect(rect.xMin, rect.yMax, m_ButtonWidth, m_Padding);
        }
    }

    void SetActiveFace(GameObject activeFace)
    {
        foreach (var face in m_Faces)
        {
            face.SetActive(activeFace == face);
        }
    }
}
