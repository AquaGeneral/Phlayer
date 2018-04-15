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
        private static string path;
        private const int maxSearchLineCount = 15;

        private static readonly string[] searchFileNames = {
            "Generator.cs", "Settings.cs", "Casing.cs"
        };

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            Settings.casing.Value = (Casing)EditorGUILayout.EnumPopup("Field Casing", Settings.casing.Value);
            Settings.skipBuiltinLayers.Value = EditorGUILayout.Toggle("Skip Builtin Layers", Settings.skipBuiltinLayers.Value);

            if(GUILayout.Button("Force Run")) {

            }

            if(GUILayout.Button("Restore Defaults")) {

            }
        }
    }
}