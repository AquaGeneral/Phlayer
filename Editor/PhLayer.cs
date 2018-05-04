using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhlayerTool {
    internal static class Phlayer {
        private const int maxSearchLineCount = 12;

        public static string mainDirectory; // Local path-space, contained with the Assets folder
        private static string settingsPath; // Local path-space, contained with the Assets folder
        public static SettingsError errorState;
        public static Settings settings;
        
        internal static bool InitializeSettings() {
            if(string.IsNullOrEmpty(mainDirectory) == false) return true;

            errorState = SettingsError.None;

            int directoryCount = 0;
            string line;
            foreach(string phLayerDirectory in Directory.GetDirectories(Application.dataPath, "*Phlayer", SearchOption.AllDirectories)) {
                directoryCount++;
                foreach(string filePath in Directory.GetFiles(phLayerDirectory, "*.cs", SearchOption.AllDirectories)) {
                    using(StreamReader sr = new StreamReader(filePath)) {
                        int lineCount = 0;
                        while((line = sr.ReadLine()) != null) {
                            if(line.StartsWith("namespace JesseStiller.PhlayerTool {", StringComparison.Ordinal)) {
                                mainDirectory = Utilities.GetLocalPathFromAbsolutePath(phLayerDirectory);
                                LoadSettings();
                                return true;
                            }

                            if(lineCount++ > maxSearchLineCount) break;
                        }
                    }
                }
            }

            if(directoryCount == 0) {
                errorState = SettingsError.NoDirectory;
            } else {
                errorState = SettingsError.NoValidFile;
            }
            return false;
        }

        private static void LoadSettings() {
            settingsPath = "Assets/" + mainDirectory + "/Settings.asset";
            if(File.Exists(settingsPath)) {
                settings = AssetDatabase.LoadAssetAtPath<Settings>(settingsPath);
            } else {
                CreateNewSettings();
            };
        }

        internal static void CreateNewSettings() {
            settings = ScriptableObject.CreateInstance<Settings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
        }
    }
}