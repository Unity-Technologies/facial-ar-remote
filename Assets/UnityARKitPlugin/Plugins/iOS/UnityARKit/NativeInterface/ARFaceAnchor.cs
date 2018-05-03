using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using AOT;

namespace UnityEngine.XR.iOS
{

	public static class ARBlendShapeLocation
	{
		 public const string  BrowDownLeft        =   "browDown_L";
	 	 public const string  BrowDownRight       =   "browDown_R";
		 public const string  BrowInnerUp         =   "browInnerUp";
		 public const string  BrowOuterUpLeft     =   "browOuterUp_L";
		 public const string  BrowOuterUpRight    =   "browOuterUp_R";
		 public const string  CheekPuff           =   "cheekPuff";
		 public const string  CheekSquintLeft     =   "cheekSquint_L";
		 public const string  CheekSquintRight    =   "cheekSquint_R";
		 public const string  EyeBlinkLeft        =   "eyeBlink_L";
		 public const string  EyeBlinkRight       =   "eyeBlink_R";
		 public const string  EyeLookDownLeft     =   "eyeLookDown_L";
		 public const string  EyeLookDownRight    =   "eyeLookDown_R";
		 public const string  EyeLookInLeft       =   "eyeLookIn_L";
		 public const string  EyeLookInRight      =   "eyeLookIn_R";
		 public const string  EyeLookOutLeft      =   "eyeLookOut_L";
		 public const string  EyeLookOutRight     =   "eyeLookOut_R";
		 public const string  EyeLookUpLeft       =   "eyeLookUp_L";
		 public const string  EyeLookUpRight      =   "eyeLookUp_R";
		 public const string  EyeSquintLeft       =   "eyeSquint_L";
		 public const string  EyeSquintRight      =   "eyeSquint_R";
		 public const string  EyeWideLeft         =   "eyeWide_L";
		 public const string  EyeWideRight        =   "eyeWide_R";
		 public const string  JawForward          =   "jawForward";
		 public const string  JawLeft             =   "jawLeft";
		 public const string  JawOpen             =   "jawOpen";
		 public const string  JawRight            =   "jawRight";
		 public const string  MouthClose          =   "mouthClose";
		 public const string  MouthDimpleLeft     =   "mouthDimple_L";
		 public const string  MouthDimpleRight    =   "mouthDimple_R";
		 public const string  MouthFrownLeft      =   "mouthFrown_L";
		 public const string  MouthFrownRight     =   "mouthFrown_R";
		 public const string  MouthFunnel         =   "mouthFunnel";
		 public const string  MouthLeft           =   "mouthLeft";
		 public const string  MouthLowerDownLeft  =   "mouthLowerDown_L";
		 public const string  MouthLowerDownRight =   "mouthLowerDown_R";
		 public const string  MouthPressLeft      =   "mouthPress_L";
		 public const string  MouthPressRight     =   "mouthPress_R";
		 public const string  MouthPucker         =   "mouthPucker";
		 public const string  MouthRight          =   "mouthRight";
		 public const string  MouthRollLower      =   "mouthRollLower";
		 public const string  MouthRollUpper      =   "mouthRollUpper";
		 public const string  MouthShrugLower     =   "mouthShrugLower";
		 public const string  MouthShrugUpper     =   "mouthShrugUpper";
		 public const string  MouthSmileLeft      =   "mouthSmile_L";
		 public const string  MouthSmileRight     =   "mouthSmile_R";
		 public const string  MouthStretchLeft    =   "mouthStretch_L";
		 public const string  MouthStretchRight   =   "mouthStretch_R";
		 public const string  MouthUpperUpLeft    =   "mouthUpperUp_L";
		 public const string  MouthUpperUpRight   =   "mouthUpperUp_R";
		 public const string  NoseSneerLeft       =   "noseSneer_L";
		 public const string  NoseSneerRight      =   "noseSneer_R";

		public static readonly List<string> Locations = new List<string>
		{
			BrowDownLeft,
			BrowDownRight,
			BrowInnerUp,
			BrowOuterUpLeft,
			BrowOuterUpRight,
			CheekPuff,
			CheekSquintLeft,
			CheekSquintRight,
			EyeBlinkLeft,
			EyeBlinkRight,
			EyeLookDownLeft,
			EyeLookDownRight,
			EyeLookInLeft,
			EyeLookInRight,
			EyeLookOutLeft,
			EyeLookOutRight,
			EyeLookUpLeft,
			EyeLookUpRight,
			EyeSquintLeft,
			EyeSquintRight,
			EyeWideLeft,
			EyeWideRight,
			JawForward,
			JawLeft,
			JawOpen,
			JawRight,
			MouthClose,
			MouthDimpleLeft,
			MouthDimpleRight,
			MouthFrownLeft,
			MouthFrownRight,
			MouthFunnel,
			MouthLeft,
			MouthLowerDownLeft,
			MouthLowerDownRight,
			MouthPressLeft,
			MouthPressRight,
			MouthPucker,
			MouthRight,
			MouthRollLower,
			MouthRollUpper,
			MouthShrugLower,
			MouthShrugUpper,
			MouthSmileLeft,
			MouthSmileRight,
			MouthStretchLeft,
			MouthStretchRight,
			MouthUpperUpLeft,
			MouthUpperUpRight,
			NoseSneerLeft,
			NoseSneerRight
		};
	}


	public struct UnityARFaceGeometry
	{
		public int vertexCount;
		public IntPtr vertices;
		public int textureCoordinateCount;
		public IntPtr textureCoordinates;
		public int triangleCount;
		public IntPtr triangleIndices;

	}

	public struct UnityARFaceAnchorData
	{

		public IntPtr ptrIdentifier;

		/**
 		The transformation matrix that defines the anchor's rotation, translation and scale in world coordinates.
		 */
		public UnityARMatrix4x4 transform;

#if UNITY_EDITOR
		public string identifierStr { get { return "id"; } }
#else
		public string identifierStr { get { return Marshal.PtrToStringAuto(this.ptrIdentifier); } }
		public UnityARFaceGeometry faceGeometry;
#endif
		public IntPtr blendShapes;

	};

#if !UNITY_EDITOR && UNITY_IOS
	public class ARFaceGeometry
	{
		const int k_VertexCount = 1220;
		const int k_TriangleCount = 2304;
		const int k_IndicesPerTriangle = 3;
		const int k_IndexCount = k_TriangleCount * k_IndicesPerTriangle;
		const int k_FloatsPerVertex = 4;
		const int k_FloatsPerTextureCoordinate = 2;
		const int k_VertexFloatCount = k_VertexCount * k_FloatsPerVertex;
		const int k_TextureCoordinateFloatCount = k_VertexCount * k_FloatsPerTextureCoordinate;

		internal UnityARFaceGeometry uFaceGeometry;

		// Local method use only -- created here to reduce garbage collection
		static readonly float[] k_WorkVertices = new float[k_VertexFloatCount];
		static readonly Vector3[] k_Vertices = new Vector3[k_VertexCount];
		static readonly float[] k_WorkTextureCoordinates = new float[k_VertexFloatCount];
		static readonly Vector2[] k_TextureCoordinates = new Vector2[k_VertexCount];
		static readonly short[] k_WorkIndices = new short[k_IndexCount];
		static readonly int[] k_Indices = new int[k_IndexCount];

		public int vertexCount { get { return uFaceGeometry.vertexCount; } }
		public int triangleCount {  get  { return uFaceGeometry.triangleCount; } }
		public int textureCoordinateCount { get { return uFaceGeometry.textureCoordinateCount; } }

		public Vector3 [] vertices { get { return MarshalVertices(uFaceGeometry.vertices); } }
		public Vector2 [] textureCoordinates { get { return MarshalTexCoords(uFaceGeometry.textureCoordinates); } }
		public int [] triangleIndices { get { return MarshalIndices(uFaceGeometry.triangleIndices); } }

		Vector3 [] MarshalVertices(IntPtr ptrFloatArray)
		{
			Marshal.Copy (ptrFloatArray, k_WorkVertices, 0, k_VertexFloatCount);

			for (var count = 0; count < k_VertexCount; count++)
			{
				var index = count * k_FloatsPerVertex;
				k_Vertices[count].x =  k_WorkVertices[index];
				k_Vertices[count].y =  k_WorkVertices[index + 1];
				k_Vertices[count].z = -k_WorkVertices[index + 2];
			}

			return k_Vertices;
		}

		int [] MarshalIndices(IntPtr ptrIndices)
		{
			Marshal.Copy (ptrIndices, k_WorkIndices, 0, k_IndexCount);

			for (var count = 0; count < k_IndexCount; count+=3) {
				//reverse winding order
				k_Indices[count]     = k_WorkIndices[count];
				k_Indices[count + 1] = k_WorkIndices[count + 2];
				k_Indices[count + 2] = k_WorkIndices[count + 1];
			}

			return k_Indices;
		}

		Vector2 [] MarshalTexCoords(IntPtr ptrTexCoords)
		{
			Marshal.Copy (ptrTexCoords, k_WorkTextureCoordinates, 0, k_TextureCoordinateFloatCount);

			for (var count = 0; count < k_VertexCount; count++)
			{
				var index = count * 2;
				k_TextureCoordinates[count].x = k_WorkTextureCoordinates[index];
				k_TextureCoordinates[count].y = k_WorkTextureCoordinates[index + 1];
			}

			return k_TextureCoordinates;
		}
	}


	public class ARFaceAnchor
	{
		// Local method use only -- created here to reduce garbage collection
		static readonly ARFaceGeometry k_FaceGeometry = new ARFaceGeometry();

		private UnityARFaceAnchorData faceAnchorData;
		private static Dictionary<string, float> blendshapesDictionary;

		public ARFaceAnchor (UnityARFaceAnchorData ufad)
		{
			faceAnchorData = ufad;
			if (blendshapesDictionary == null) {
				blendshapesDictionary = new Dictionary<string, float> ();
			}
		}


		public string identifierStr { get { return faceAnchorData.identifierStr; } }

		public Matrix4x4 transform {
			get {
				Matrix4x4 matrix = new Matrix4x4 ();
				matrix.SetColumn (0, faceAnchorData.transform.column0);
				matrix.SetColumn (1, faceAnchorData.transform.column1);
				matrix.SetColumn (2, faceAnchorData.transform.column2);
				matrix.SetColumn (3, faceAnchorData.transform.column3);
				return matrix;
			}
		}

		public ARFaceGeometry faceGeometry
		{
			get
			{
				k_FaceGeometry.uFaceGeometry = faceAnchorData.faceGeometry;
				return k_FaceGeometry;
			}
		}

		public Dictionary<string, float> blendShapes { get { return GetBlendShapesFromNative(faceAnchorData.blendShapes); } }

		delegate void DictionaryVisitorHandler(IntPtr keyPtr, float value);

		[DllImport("__Internal")]
		private static extern void GetBlendShapesInfo(IntPtr ptrDic, DictionaryVisitorHandler handler);

		Dictionary<string, float> GetBlendShapesFromNative(IntPtr blendShapesPtr)
		{
			blendshapesDictionary.Clear ();
			GetBlendShapesInfo (blendShapesPtr, AddElementToManagedDictionary);
			return blendshapesDictionary;
		}

		[MonoPInvokeCallback(typeof(DictionaryVisitorHandler))]
		static void AddElementToManagedDictionary(IntPtr keyPtr, float value)
		{
			string key = Marshal.PtrToStringAuto(keyPtr);
			blendshapesDictionary.Add(key, value);
		}
	}
#endif
}
