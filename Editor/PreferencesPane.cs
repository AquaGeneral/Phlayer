using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        private static readonly int textFieldWithDefaultId = "PhLayer.TextFieldWithDefault".GetHashCode();

        private static class Styles {
            internal static GUIStyle greyItalicLabel, wordWrappedTextArea;
            internal static void Initialize() {
                if(greyItalicLabel == null) {
                    greyItalicLabel = new GUIStyle(GUI.skin.label);
                    greyItalicLabel.padding = GUI.skin.textField.padding;
                    greyItalicLabel.margin = GUI.skin.textField.margin;
                    greyItalicLabel.fontStyle = FontStyle.Italic;
                }
                if(wordWrappedTextArea == null) {
                    wordWrappedTextArea = new GUIStyle(GUI.skin.textArea);
                    wordWrappedTextArea.wordWrap = true;
                }
            }
        }

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            Styles.Initialize();
            PhLayer.InitializeSettings();

            switch(PhLayer.errorState) {
                case SettingsError.NoDirectory:
                    EditorGUILayout.HelpBox("There is no valid directory named PhLayer - do not rename the directory of PhLayer.", MessageType.Error);
                    break;
                case SettingsError.NoValidFile:
                    EditorGUILayout.HelpBox("PhLayer somehow couldn't find the main location of its files. Make sure you did not modify PhLayer's code, nor directory names.", MessageType.Error);
                    break;
            }

            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = 130f;

            EditorGUI.BeginChangeCheck();
            PhLayer.settings.className = TextFieldWithDefault("Class Name", PhLayer.settings.className, "Layers");
            PhLayer.settings.classNamespace = EditorGUILayout.TextField("Class Namespace", PhLayer.settings.classNamespace);
            PhLayer.settings.casing = (Casing)EditorGUILayout.EnumPopup("Field Casing", PhLayer.settings.casing);
            PhLayer.settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", PhLayer.settings.skipBuiltinLayers);
            
            EditorGUILayout.BeginHorizontal();
            PhLayer.settings.outputPath = TextAreaWithDefault("Output Path", PhLayer.settings.outputPath, GetLocalPathFromAbsolutePath(PhLayer.mainDirectory));
            if(GUILayout.Button("Browse…", GUILayout.Width(70f))) {
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

        
        private static string TextFieldWithDefault(string label, string value, string defaultValue) {
            string newValue = EditorGUILayout.TextField(label, value);
            Rect rect = GUILayoutUtility.GetLastRect();
            if(string.IsNullOrEmpty(value)) {
                if(Event.current.type == EventType.Repaint) {
                    Styles.greyItalicLabel.Draw(new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, 200f, rect.height), defaultValue, false, false, false, false);
                }
                
            }
            return newValue;
        }

        private static string TextAreaWithDefault(string label, string value, string defaultValue) {
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(29f));
            Rect textAreaRect = EditorGUI.PrefixLabel(controlRect, new GUIContent(label));
            string newValue = EditorGUI.TextArea(textAreaRect, value, Styles.wordWrappedTextArea);

            if(string.IsNullOrEmpty(value)) {
                if(Event.current.type == EventType.Repaint) {
                    Styles.greyItalicLabel.Draw(textAreaRect, defaultValue, false, false, false, false);
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