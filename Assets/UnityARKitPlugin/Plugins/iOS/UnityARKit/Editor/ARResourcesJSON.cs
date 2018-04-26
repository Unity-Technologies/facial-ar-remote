using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.XR.iOS
{
	[Serializable]
	public class ARResourceGroupInfo
	{
		public int version;
		public string author;
	}

	[Serializable]
	public class ARResourceGroupResource
	{
		public string filename;
	}

	[Serializable]
	public class ARResourceGroupContents
	{
		public ARResourceGroupInfo info;
		public ARResourceGroupResource [] resources;
	}

	[Serializable]
	public class ARResourceInfo
	{
		public int version;
		public string author;
	}
		
	[Serializable]
	public class ARResourceProperties
	{
		public float width;
	}

	[Serializable]
	public class ARResourceImage
	{
		public string idiom;
		public string filename;
	}

	[Serializable]
	public class ARResourceContents
	{
		public ARResourceImage [] images;
		public ARResourceInfo info;
		public ARResourceProperties properties;
	}
}