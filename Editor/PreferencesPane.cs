using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
* TODO:
* - Validate class namespace,
* - Validate class name
*/

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
                    EditorGUILayout.HelpBox("There is no valid directory named PhLayer - do not rename the directory that PhLayer is contained within.", MessageType.Error);
                    break;
                case SettingsError.NoValidFile:
                    EditorGUILayout.HelpBox("PhLayer somehow couldn't find the main location of its files. Make sure you did not modify PhLayer's code, nor directory names.", MessageType.Error);
                    break;
            }

            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = 130f;

            EditorGUI.BeginChangeCheck();
            PhLayer.settings.className = TextFieldWithDefault("Class Name*", PhLayer.settings.className, "Layers");
            PhLayer.settings.className = ValidatedIdentifier(PhLayer.settings.className);
            PhLayer.settings.classNamespace = EditorGUILayout.TextField("Class Namespace", PhLayer.settings.classNamespace);
            PhLayer.settings.casing = (Casing)EditorGUILayout.EnumPopup("Field Casing", PhLayer.settings.casing);
            PhLayer.settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", PhLayer.settings.skipBuiltinLayers);
            PhLayer.settings.lineEndings = (LineEndings)EditorGUILayout.EnumPopup("Line Endings", PhLayer.settings.lineEndings);
            PhLayer.settings.curlyBracketPreference = (CurlyBracketPreference)EditorGUILayout.EnumPopup("Curly Bracket Opening", PhLayer.settings.curlyBracketPreference);

            EditorGUILayout.BeginHorizontal();
            PhLayer.settings.outputDirectory = TextAreaWithDefault("Output Directory*", PhLayer.settings.outputDirectory, "Assets\\");
            if(GUILayout.Button("Browse…", GUILayout.Width(70f), GUILayout.Height(22f))) {
                string chosenPath = EditorUtility.OpenFolderPanel("PhLayer", GetOutputDirectory(), string.Empty);
                if(string.IsNullOrEmpty(chosenPath) == false) PhLayer.settings.outputDirectory = GetLocalPathFromAbsolutePath(chosenPath);
            }
            EditorGUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck()) {
                PhLayer.SaveSettings();
            }

            using(new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                EditorStyles.centeredGreyMiniLabel.richText = true;
                GUILayout.Label("* Values in <i>italics</i> represent default values", EditorStyles.centeredGreyMiniLabel);
                EditorStyles.centeredGreyMiniLabel.richText = false;
            }

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Force Generate", GUILayout.Width(140f), GUILayout.Height(22f))) {
                Generator.Generate();
            }

            if(GUILayout.Button("Restore Defaults", GUILayout.Width(140f), GUILayout.Height(22f))) {

            }

            EditorGUILayout.EndHorizontal();
        }

        private static string ValidatedIdentifier(string s) {
            // Letter upercase  u0041 to u1e921
            // Letter lowercase u0061 to u1e943

            StringBuilder sb = new StringBuilder();

            for(int c = 0; c < s.Length; c++) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(s[c]);

                if(uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter) {
                    sb.Append(s[c]);
                } else {
                    sb.Append('_');
                }

                //if(s[c] >= '\u0041' && s[c] <= '\uFF3A') {
                //    sb.Append(s[c]);
                //} else {
                //    sb.Append('_');
                //}
            }
            return sb.ToString();
        }

        private static string GetOutputDirectory() {
            if(string.IsNullOrEmpty(PhLayer.settings.outputDirectory)) {
                return GetLocalPathFromAbsolutePath(PhLayer.mainDirectory);
            } else {
                return PhLayer.settings.outputDirectory;
            }
        }

        private static string TextFieldWithDefault(string label, string value, string defaultValue) {
            string newValue = EditorGUILayout.TextField(label, value);

            //int controlId = GUIUtility.GetControlID("TextField".GetHashCode(), rect);
            //Event controlEvent = Event.current.GetTypeForControl(controlId);

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