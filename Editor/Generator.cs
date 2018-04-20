using Microsoft.CSharp;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
* TODO: 
* - Validate non-typical characters such as the copyright symbol when it comes to naming them in the class.
*/

namespace JesseStiller.PhLayerTool {
    //Call generate each time Unity is opened
    [InitializeOnLoad]
    public static class Startup {
        static Startup() {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            //PhLayerGenerator.Generate();
            Debug.Log("Startup");
        }
    }

    public class Generator : AssetPostprocessor {
        private const string windowsLineEnding = "\r\n";
        private const string unixLineEnding = "\n";
        private static string header = "// Auto-generated based on the TagManager settings by Jesse Stiller's PhLayer Unity extension.";
        // TODO: Is a text writer faster?
        private static StringBuilder sb = new StringBuilder(512);
        private static byte indentation;

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

            string className = string.IsNullOrEmpty(PhLayer.settings.className) ? "Layers" : PhLayer.settings.className;
            string outputDirectory = string.IsNullOrEmpty(PhLayer.settings.outputDirectory) ? "Assets\\" : PhLayer.settings.outputDirectory;
            string localFilePath = Path.Combine(outputDirectory, className + ".g.cs");
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

            // Reset state
            sb.Clear();
            indentation = 0;
            
            AppendLine(header);

            // Namespace
            if(string.IsNullOrEmpty(PhLayer.settings.classNamespace) == false) {
                AppendLineWithCurlyBracket("namespace " + PhLayer.settings.classNamespace);
            }

            // Class declaration
            AppendLineWithCurlyBracket("public static class " + PhLayer.settings.className);
            
            for(int i = PhLayer.settings.skipBuiltinLayers ? 8 : 0; i < 32; i++) {
                string layerName = UnityEngine.LayerMask.LayerToName(i);
                if(string.IsNullOrEmpty(layerName)) continue;

                layerName = layerName.Replace(" ", "");

                if(char.IsDigit(layerName[0])) {
                    layerName = "_" + layerName;
                }
                
                switch(PhLayer.settings.casing) {
                    case Casing.Camel:
                        layerName = char.ToLowerInvariant(layerName[0]) + layerName.Substring(1);
                        break;
                    case Casing.Pascal:
                        throw new NotImplementedException();
                        break;
                    case Casing.CapsLock:
                        layerName = layerName.ToUpperInvariant();
                        break;
                }

                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                layerName = codeProvider.CreateValidIdentifier(layerName);
                
                AppendLine(string.Format("public const int {0} = {1};", layerName, i));
            }

            // Write all ending curly brakets
            while(indentation-- > 0) {
                AppendLine("}");
            }

            File.WriteAllText(absoluteFilePath, sb.ToString());

            AssetDatabase.ImportAsset(localFilePath);
        }

        private static void AppendLineWithCurlyBracket(string v) {
            Append(v);
            if(PhLayer.settings.curlyBracketPreference == CurlyBracketPreference.NewLine) {
                AppendLine("");
                AppendLine("{");
            } else {
                sb.Append(" {");
                AppendLine("");
            }

            Indent();
        }

        private static void Append(string s) {
            Debug.Assert(indentation < 10);
            for(int i = 0; i < indentation; i++) sb.Append('\t');
            sb.Append(s);
        }

        private static void AppendLine(string s) {
            Append(s);
            if(PhLayer.settings.lineEndings == LineEndings.Windows) {
                sb.Append(windowsLineEnding);
            } else {
                sb.Append(unixLineEnding);
            }
        }

        private static void Indent() {
            indentation++;
        }

        private static void Unindent() {
            indentation--;
            Debug.Assert(indentation >= 0);
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
