using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    internal static class PhLayer {
        private const int maxSearchLineCount = 12;

        public static string mainDirectory;
        public static string settingsPath;
        public static SettingsError errorState;
        public static Settings settings;
        
        internal static void InitializeSettings() {
            if(string.IsNullOrEmpty(mainDirectory) == false) return;

            errorState = SettingsError.None;

            int lineCount;
            int directoryCount = 0;
            string line;
            IEnumerable<string> phLayerDirectories = Directory.EnumerateDirectories(Application.dataPath, "*PhLayer", SearchOption.AllDirectories);
            foreach(string phLayerDirectory in phLayerDirectories) {
                directoryCount++;
                foreach(string filePath in Directory.EnumerateFiles(phLayerDirectory, "*.cs", SearchOption.AllDirectories)) {
                    using(StreamReader sr = new StreamReader(filePath)) {
                        lineCount = 0;
                        while((line = sr.ReadLine()) != null) {
                            if(line.StartsWith("namespace JesseStiller.PhLayerTool {", StringComparison.Ordinal)) {
                                mainDirectory = phLayerDirectory;
                                LoadSettings();
                                return;
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
        }

        private static void LoadSettings() {
            settingsPath = Path.Combine(mainDirectory, "Settings.json");
            if(File.Exists(settingsPath)) {
                string settingsJSON = File.ReadAllText(settingsPath);
                try {
                    settings = JsonUtility.FromJson<Settings>(settingsJSON);
                } catch(Exception e) {
                    Debug.LogError(e);
                }
            } else {
                settings = new Settings();
            }
        }

        internal static void SaveSettings() {
            Debug.Assert(settings != null);
            Debug.Assert(string.IsNullOrEmpty(settingsPath) == false);

            try {
                File.WriteAllText(settingsPath, EditorJsonUtility.ToJson(settings, true));
            } catch(Exception e) {
                Debug.LogError(e);
            }
        }
    }
}