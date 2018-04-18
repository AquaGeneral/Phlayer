using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        private static readonly int textFieldWithDefaultId = "PhLayer.TextFieldWithDefault".GetHashCode();

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            PhLayer.InitializeSettings();

            switch(PhLayer.errorState) {
                case ErrorState.NoDirectory:
                    EditorGUILayout.HelpBox("There is no valid directory named PhLayer - do not rename the directory of PhLayer.", MessageType.Error);
                    break;
                case ErrorState.NoValidFile:
                    EditorGUILayout.HelpBox("PhLayer somehow couldn't find the main location of its files. Make sure you did not modify PhLayer's code, nor directory names.", MessageType.Error);
                    break;
            }

            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = 150f;

            EditorGUI.BeginChangeCheck();
            PhLayer.settings.className = TextFieldWithDefault("Class Name", PhLayer.settings.className, "Layers");
            PhLayer.settings.classNamespace = EditorGUILayout.TextField("Class Namespace", PhLayer.settings.classNamespace);
            PhLayer.settings.casing = (Casing)EditorGUILayout.EnumPopup("Field Casing", PhLayer.settings.casing);
            PhLayer.settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", PhLayer.settings.skipBuiltinLayers);
            
            EditorGUILayout.BeginHorizontal();
            PhLayer.settings.outputPath = TextFieldWithDefault("Output Path", PhLayer.settings.outputPath, GetLocalPathFromAbsolutePath(PhLayer.mainDirectory));
            if(GUILayout.Button("Choose…", GUILayout.Width(70f))) {
                string chosenPath = EditorUtility.SaveFilePanelInProject("PhLayer", "Settings.json", "json", "");
                if(string.IsNullOrEmpty(chosenPath) == false) PhLayer.settings.outputPath = chosenPath;
            }
            EditorGUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck()) {
                PhLayer.SaveSettings();
            }

            if(GUILayout.Button("Force Generate")) {
                Generator.Generate();
            }

            if(GUILayout.Button("Restore Defaults")) {

            }
        }

        private static GUIStyle greyItalicLabelStyle;
        private static string TextFieldWithDefault(string label, string value, string defaultValue) {
            if(greyItalicLabelStyle == null) {
                greyItalicLabelStyle = new GUIStyle(GUI.skin.label);
                greyItalicLabelStyle.fontStyle = FontStyle.Italic;
            }

            string newValue = EditorGUILayout.TextField(label, value);
            Rect rect = GUILayoutUtility.GetLastRect();
            if(string.IsNullOrEmpty(value)) {
                if(Event.current.type == EventType.Repaint) {
                    GUI.enabled = false;
                    greyItalicLabelStyle.Draw(new Rect(rect.x + EditorGUIUtility.labelWidth + EditorStyles.label.padding.left, rect.y, 200f, rect.height), defaultValue, false, false, false, false);
                    GUI.enabled = true;
                }
                
            }
            return newValue;
        }

        internal static string GetLocalPathFromAbsolutePath(string absolutePath) {
            int indexOfAssets = absolutePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

            if(indexOfAssets == -1) {
                throw new ArgumentException("The 'assetsPath' parameter must contain 'Assets/'");
            }
            return absolutePath.Remove(0, indexOfAssets);
        }
    }
}