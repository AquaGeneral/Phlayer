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
        private const string generatedClassName = "Layers";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach(string str in importedAssets) {
                if(str.Equals("ProjectSettings/TagManager.asset", StringComparison.OrdinalIgnoreCase)) {
                    Generate();
                }
            }
        }
        
        [MenuItem("Jesse Stiller/Update Layer Class")]
        internal static void Generate() {
            if(PreferencesPane.settings == null) {
                PreferencesPane.InitializeSettings();
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// Auto-generated based on the TagManager settings by Jesse Stiller's PhLayer Unity extension." + Environment.NewLine);

            sb.AppendLine($"public static class {generatedClassName} {{");
            
            for(int i = 0; i < 32; i++) {
                string layerName = UnityEngine.LayerMask.LayerToName(i);
                if(string.IsNullOrEmpty(layerName)) continue;

                layerName = layerName.Replace(" ", "");

                if(char.IsDigit(layerName[0])) {
                    layerName = "layer" + layerName;
                }
                
                switch(PreferencesPane.settings.casing) {
                    case Casing.Camel:
                        layerName = char.ToLowerInvariant(layerName[0]) + layerName.Substring(1);
                        break;
                    case Casing.Pascal:
                        break;
                    case Casing.Caps:
                        layerName = layerName.ToUpperInvariant();
                        break;
                }

                if(layerName == "default") layerName = "@default";

                sb.AppendLine(string.Format("\tpublic const int {0} = {1};", layerName, i));
            }

            sb.AppendLine("}");

            File.WriteAllText(Application.dataPath + $"/Scripts/{generatedClassName}.cs", sb.ToString());

            AssetDatabase.ImportAsset($"Assets/Scripts/{generatedClassName}.cs");
        }
    }
}