using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    // Call generate each time Unity is opened
    //[InitializeOnLoad]
    //public static class Startup {
    //    static Startup() {
    //        PhLayerGenerator.Generate();
    //        Debug.Log("Generate");
    //    }
    //}

    public class Generator : AssetPostprocessor {
        private static string header = "// Auto-generated based on the TagManager settings by Jesse Stiller's PhLayer Unity extension.\n";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach(string str in importedAssets) {
                if(str.Equals("ProjectSettings/TagManager.asset", StringComparison.OrdinalIgnoreCase)) {
                    Generate();
                }
            }
        }
        
        [MenuItem("Jesse Stiller/Update Layer Class")]
        internal static void Generate() {
            PhLayer.InitializeSettings();

            string localFilePath = Path.Combine(PhLayer.settings.outputPath, PhLayer.settings.className + ".g.cs");
            string absoluteFilePath = GetAbsolutePathFromLocalPath(localFilePath);

            // Make sure that we are writing to one of our own files if already present, and not something created by a anyone/anything else.
            if(File.Exists(absoluteFilePath)) {
                using(StreamReader sr = new StreamReader(absoluteFilePath)) {
                    if(sr.ReadLine().StartsWith(header, StringComparison.Ordinal) == false) {
                        bool overwriteAnyway = EditorUtility.DisplayDialog("PhLayer", "PhLayer was going to update the generated layers physics layers class, but it is going to overwrite a non-matching file at:\n" + 
                            absoluteFilePath, "Overwrite anyway", "Don't overwrite");
                        if(overwriteAnyway == false) return;
                    }
                }
            }

            // TODO: Is a text writer faster?
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(header);

            sb.AppendLine("public static class " + PhLayer.settings.className + " {");
            
            for(int i = PhLayer.settings.skipBuiltinLayers ? 8 : 0; i < 32; i++) {
                string layerName = UnityEngine.LayerMask.LayerToName(i);
                if(string.IsNullOrEmpty(layerName)) continue;

                layerName = layerName.Replace(" ", "");

                if(char.IsDigit(layerName[0])) {
                    layerName = "layer" + layerName;
                }
                
                switch(PhLayer.settings.casing) {
                    case Casing.Camel:
                        layerName = char.ToLowerInvariant(layerName[0]) + layerName.Substring(1);
                        break;
                    case Casing.Pascal:
                        throw new NotImplementedException();
                        break;
                    case Casing.Caps:
                        layerName = layerName.ToUpperInvariant();
                        break;
                }

                if(layerName == "default") layerName = "@default";

                sb.AppendLine(string.Format("\tpublic const int {0} = {1};", layerName, i));
            }

            sb.AppendLine("}");

            File.WriteAllText(absoluteFilePath, sb.ToString());

            AssetDatabase.ImportAsset(localFilePath);
        }

        /// <summary>
        /// Get the absolute path from a local path.
        /// </summary>
        /// <param name="localPath">The path contained within the "Assets" direction. Eg: "Models/Model.fbx"</param>
        /// <example>Passing "Models/Model.fbx" will return "C:/Users/John/MyProject/Assets/Models/Model.fbx"</example>
        /// <returns>Returns the absolute/full system path from the local "Assets" inclusive path.</returns>
        internal static string GetAbsolutePathFromLocalPath(string localPath) {
            return Application.dataPath.Remove(Application.dataPath.Length - 6, 6) + localPath;
        }
    }
}