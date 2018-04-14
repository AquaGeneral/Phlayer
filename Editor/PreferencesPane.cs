using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.IO;
using System;
using UnityEngine.Profiling;
using System.Text;

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        internal static Settings settings;

        private static string path;
        private static StringBuilder sb = new StringBuilder(16);
        private const int maxSearchLineCount = 15;

        private static readonly string[] searchFileNames = {
            "Generator.cs", "Settings.cs", "Casing.cs"
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
            MonoScript[] monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();

            foreach(MonoScript o in monoScripts) {
                string filePath = AssetDatabase.GetAssetPath(o);
                if(StringAStartsWithB(filePath, "Library")) continue;
                string fileName = GetFileNameSb(filePath);
                bool validFileName = false;
                for(int fn = 0; fn < searchFileNames.Length; fn++) {
                    if(filePath.Equals(searchFileNames[fn], StringComparison.OrdinalIgnoreCase)) {
                        validFileName = true;
                        break;
                    }
                }
                if(validFileName == false) continue;

                //if(File.Exists(filePath) == false) continue;

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
                    lastSlashIndex = c;
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
            GetMainDirectory();

            //string terrainFormerPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(terrainFormerComponent));
            if(settings == null) {
                InitializeSettings();
            }

            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            settings.casing = (Casing)EditorGUILayout.EnumPopup("Field Casing", settings.casing);
            settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", settings.skipBuiltinLayers);
        }
    }
}