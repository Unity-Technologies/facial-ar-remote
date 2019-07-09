using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace PerformanceRecorder
{
    [ScriptedImporter(1, "arstream")]
    public class ARStreamImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var asset = ScriptableObject.CreateInstance<ARStreamAsset>();
            asset.bytes = bytes;
            ctx.AddObjectToAsset("ARStreamAsset", asset);
            ctx.SetMainObject(asset);
        }
    }
}
