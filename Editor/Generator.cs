using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
* TODO: 
* - Validate non-typical characters such as the copyright symbol when it comes to naming them in the class.
*/

namespace JesseStiller.PhLayerTool {
    public class Generator : AssetPostprocessor {
        private const string windowsLineEnding = "\r\n";
        private const string unixLineEnding = "\n";
        private const string header = "// Auto-generated based on the TagManager settings by Jesse Stiller's PhLayer Unity extension.";
        private const string previewHeader = "// Auto-generated.";
        private static StringBuilder sb = new StringBuilder(512);
        private static readonly string[] indentatorsArray = { " ", "  ", "   ", "    ", "\t" };
        private static byte indentation;

        private static string indentator;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach(string str in importedAssets) {
                if(str.Equals("ProjectSettings/TagManager.asset", StringComparison.OrdinalIgnoreCase)) {
                    GenerateAndSave();
                }
            }
        }

        // TODO: Get this working without creating an infinite loop
        // [InitializeOnLoadMethod]
        // private static void OnLoad() {
        //     if(EditorApplication.isPlayingOrWillChangePlaymode) return;
        //     Generator.Generate();
        // }

        [MenuItem("Assets/PhLayer/Force Class Generation")]
        internal static void GenerateAndSave() {
            PhLayer.InitializeSettings();

            List<string> layerNames = new List<string>(32);
            for(int i = PhLayer.settings.skipBuiltinLayers ? 8 : 0; i < 32; i++) {
                layerNames.Add(LayerMask.LayerToName(i));
            }
            Generate(preview: true);

            string localFilePath = GetLocalPath();
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
            File.WriteAllText(absoluteFilePath, sb.ToString());
            AssetDatabase.ImportAsset(localFilePath, ImportAssetOptions.ForceUpdate);
        }

        internal static string GetLocalPath() {
            string className = string.IsNullOrEmpty(PhLayer.settings.className) ? "Layers" : PhLayer.settings.className;
            string outputDirectory = string.IsNullOrEmpty(PhLayer.settings.outputDirectory) ? "Assets\\" : PhLayer.settings.outputDirectory;
            string extension = PhLayer.settings.appendDotGInFileName ? ".g.cs" : ".cs";
            return Path.Combine(outputDirectory, className + extension);
        }

        private static void Generate(bool preview) {
            // Don't display the tabs in the preview because they are way too wide and seemingly can't be shrinked.
            if(preview && PhLayer.settings.indentationStyle == IndentationStyle.Tabs) {
                indentator = indentatorsArray[(byte)IndentationStyle.Spaces4];
            } else {
                indentator = indentatorsArray[(byte)PhLayer.settings.indentationStyle];
            }

            // Reset state
            sb.Length = 0;
            indentation = 0;

            if(PhLayer.settings.includeHeader) {
                if(preview) AppendLine(previewHeader);
                else        AppendLine(header);
            }

            // Namespace
            if(string.IsNullOrEmpty(PhLayer.settings.classNamespace) == false) {
                AppendLineWithCurlyBracket("namespace " + PhLayer.settings.classNamespace);
            }

            // Class declaration
            string className = string.IsNullOrEmpty(PhLayer.settings.className) ? "Layers" : PhLayer.settings.className;
            AppendLineWithCurlyBracket("public static class " + className);

            for(int i = PhLayer.settings.skipBuiltinLayers ? 8 : 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);
                // TODO: Check for it being only whitespace
                if(string.IsNullOrEmpty(layerName)) continue;

                layerName = layerName.Replace(" ", "");
                string newLayerName;
                if(char.IsDigit(layerName[0])) {
                    newLayerName = "_";
                } else {
                    newLayerName = "";
                }

                for(int c = 0; c < layerName.Length; c++) {
                    if(layerName[c] == ' ' || Utilities.IsCharValidForIdentifier(layerName[c]) == false) {
                        newLayerName += '_';
                    } else {
                        newLayerName += layerName[c];
                    }
                }

                switch(PhLayer.settings.casing) {
                    case Casing.Camel:
                        newLayerName = char.ToLowerInvariant(newLayerName[0]) + newLayerName.Substring(1);
                        break;
                    case Casing.Pascal:
                        throw new NotImplementedException();
                    case Casing.CapsLock:
                        newLayerName = newLayerName.ToUpperInvariant();
                        break;
                    case Casing.CapsLockWithUnderscores:
                        break;
                }

                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                newLayerName = codeProvider.CreateValidIdentifier(newLayerName);

                AppendLine(string.Format("public const int {0} = {1};", newLayerName, i));
            }

            // Write all ending curly brakets
            while(indentation-- > 0) {
                if(indentation == 0) Append("}");
                                else AppendLine("}");
            }
        }
        

        internal static string GetPreview() {
            Settings previewSettings = PhLayer.settings.Clone();
            previewSettings.indentationStyle = IndentationStyle.Spaces4;

            Generate(preview: true);
            return sb.ToString();
        }

        private static void AppendLineWithCurlyBracket(string v) {
            Append(v);
            if(PhLayer.settings.curlyBracketOnNewLine) {
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

            for(byte i = 0; i < indentation; i++) {
                sb.Append(indentator);
            }
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
