using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        internal static Settings settings;

        private static string path;
        private static StringBuilder sb = new StringBuilder(32);
        private const int maxSearchLineCount = 15;

        private static readonly string[] searchFileNames = {
            "Generator.cs", "Settings.cs", "Casing.cs", "SavedProperty.cs"
        };

        internal static void InitializeSettings() {
            settings = new Settings();

            // Find the directory of any PhLayer script.
            string mainDirectory = GetMainDirectory();
            if(mainDirectory == null) {
                Debug.LogError("PhLayer couldn't find its main directory.");
            }
        }

        /**
        * We're down to the option of looking through every MonoScript to find a class of PhLayer since no objects are ScriptableObjects, 
        * nor MonoBehaviour, and we can't rely on CompilerServices since need to target Unity 5.0.
        */
        private static string GetMainDirectory() {
            /**
            * TODO: 
            * 1) Enumerate folders, 
            * 2) find the PhLayer folder,
            * 3) Once found, then enumerate files
            * 4) Find valid filenames (possibly not even necessary since it's in a folder called PhLayer)
            * 5) Go into the file to make sure it's one of ours
            */

            foreach(string filePath in Directory.EnumerateFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)) {
                bool validFileName = false;
                for(int fn = 0; fn < searchFileNames.Length; fn++) {
                    if(filePath.EndsWith(searchFileNames[fn], StringComparison.Ordinal)) {
                        validFileName = true;
                        break;
                    }
                }
                if(validFileName == false) continue;

                if(IsPhLayerPath(filePath) == false) continue;

                int lineCount;
                using(StreamReader sr = new StreamReader(filePath)) {
                    lineCount = 0;
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        if(line.StartsWith("namespace JesseStiller.PhLayerTool {", StringComparison.Ordinal)) {
                            return filePath;
                        }

                        if(lineCount++ > maxSearchLineCount) break;
                    }
                }
            }

            return null;
        }

        // TODO: Make this MacOS compataible (\ instead of /)?
        private static bool IsPhLayerPath(string filePath) {
            const string phLayerString = "PhLayer/";
            const string assetsString = "Assets/";

            // Make sure the path starts with "Assets/"
            for(int a = 0; a < assetsString.Length; a++) {
                if(filePath[a] != assetsString[a]) return false;
            }

            int blockIndex = 0;
            bool waitUntilNextBlock = false;
            // Check for a directory named PhLayer (case-insensitive just in case a user wants them lowercase).
            for(int c = assetsString.Length; c < filePath.Length; c++) {
                if(filePath[c] == '/') {
                    waitUntilNextBlock = false;
                }  else if(waitUntilNextBlock) {
                    continue;
                }

                if(filePath[c] != phLayerString[blockIndex]) {
                    waitUntilNextBlock = true;
                }

                blockIndex++;
                
                if(blockIndex > assetsString.Length) {
                    if(waitUntilNextBlock == false) {
                        return true;
                    } else {
                        waitUntilNextBlock = true;
                    }
                }

                if(filePath[c] == '/') {
                    blockIndex = 0;
                }
            }

            return false;
        }

        private static bool StringAStartsWithB(string a, string b) {
            int length = b.Length;
            for(int c = 0; c < length; c++) {
                if(a[c] != b[c]) return false;
            }
            return true;
        }

        private static List<char> chars = new List<char>();
        private static string GetFileName(string filePath) {
            chars.Clear();
            char ch;
            for(int c = filePath.Length - 1; c > 0; c--) {
                ch = filePath[c];
                if(ch == '/') break;
                sb.Insert(0, ch);
            }
            return sb.ToString();
        }

        static string tempString;
        private static string GetFileNameSb(string filePath) {
            //sb.Clear();
            //chars.Clear();
            tempString = "";

            int lastSlashIndex = -1;
            for(int c = filePath.Length - 1; c >= 0; c--) {
                if(filePath[c] == '/') {
                    lastSlashIndex = c + 1;
                    break;
                }
            }
            if(lastSlashIndex == -1) return filePath;

            for(int c = lastSlashIndex; c < filePath.Length; c++) {
                //chars.Add(filePath[c]);
                tempString += filePath[c];
            }
            return tempString;
        }

        private static bool DoesPathContainsFileName(string path, string fileName) {
            UInt16 pathLength = (UInt16)path.Length;

            for(int c = pathLength - fileName.Length - 1; c < pathLength; c++) {
                if(fileName[c - fileName.Length] != path[c]) return false;
            }
            return true;
        }

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            UnityEngine.Profiling.Profiler.BeginSample("GetMainDirectory");
            GetMainDirectory();
            UnityEngine.Profiling.Profiler.EndSample();
            //string terrainFormerPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(terrainFormerComponent));
            if(settings == null) {
                InitializeSettings();
            }

            foreach(MonoScript ms in Resources.FindObjectsOfTypeAll<MonoScript>()) {
                string filePath = AssetDatabase.GetAssetPath(ms);
                if(IsPhLayerPath(filePath)) { EditorGUILayout.TextField(filePath); }
            }

            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            //settings.casing = (Casing)EditorGUILayout.EnumPopup("Field Casing", settings.casing);
            //settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", settings.skipBuiltinLayers);
        }
    }
}