using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;


namespace UnityEngine.XR.iOS
{

	public struct UnityARVideoFormat  {
		public IntPtr videoFormatPtr;
		public float imageResolutionWidth;
		public float imageResolutionHeight;
		public int framesPerSecond;

		#if UNITY_EDITOR
		private static void EnumerateVideoFormats(VideoFormatEnumerator videoFormatEnumerator) {
		}
		#else
		[DllImport("__Internal")]
		private static extern void EnumerateVideoFormats(VideoFormatEnumerator videoFormatEnumerator);
		#endif

		static List<UnityARVideoFormat> videoFormatsList;

		public static List<UnityARVideoFormat> SupportedVideoFormats()
		{
			videoFormatsList = new List<UnityARVideoFormat> ();
			EnumerateVideoFormats (AddToVFList);

			return videoFormatsList;
		}

		[MonoPInvokeCallback(typeof(VideoFormatEnumerator))]
		private static void AddToVFList(UnityARVideoFormat newFormat)
		{
			Debug.Log ("New Format returned");
			videoFormatsList.Add (newFormat);
		}

	}

	public delegate void VideoFormatEnumerator(UnityARVideoFormat videoFormat);


}