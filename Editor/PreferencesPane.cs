using System;
using System.Globalization;
using System.Reflection;
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
        private static readonly MethodInfo doTextFieldMethod = typeof(EditorGUI).GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo recycledEditorField = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly int textFieldWithDefaultId = "PhLayer.TextFieldWithDefault".GetHashCode();

        private static readonly StringBuilder sb = new StringBuilder();

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
            //PhLayer.settings.className = TextFieldWithDefault("Class Name*", PhLayer.settings.className, "Layers");
            PhLayer.settings.className = ValidatedTextField("Class Name", PhLayer.settings.className, "Layers");
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

        private static bool IsCharacterValidIdentifier(char c) {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            return (uc >= UnicodeCategory.UppercaseLetter && uc <= UnicodeCategory.SpacingCombiningMark) || uc == UnicodeCategory.DecimalDigitNumber ||
                uc == UnicodeCategory.LetterNumber || uc == UnicodeCategory.Format || uc == UnicodeCategory.ConnectorPunctuation;
        }

        private static string ConvertToValidIdentifier(string s) {
            sb.Clear();
            //changed = false;
            for(int c = 0; c < s.Length; c++) {
                if(IsCharacterValidIdentifier(s[c])) {
                    sb.Append(s[c]);
                } else {
                    sb.Append('_');
                    //changed = true;
                }
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

        private static readonly int validatedTextField = "PhLayerValidatedTextField".GetHashCode();
        internal static string ValidatedTextField(string label, string text, string defaultValue) {
            Rect r = EditorGUILayout.GetControlRect();

            int controlId = GUIUtility.GetControlID(validatedTextField, FocusType.Keyboard, r);
            bool changed = false;
            switch(Event.current.GetTypeForControl(controlId)) {
                case EventType.KeyDown:
                    if(Event.current.character == 0) break; // Allow backspace, delete, etc
                    if(IsCharacterValidIdentifier(Event.current.character)) break;
                    Event.current.Use();
                    break;
                case EventType.ExecuteCommand:
                    // HACK: This changes the clipboard value, ideally it shouldn't with more complicated logic, but is it worth it?
                    if(Event.current.commandName == "Paste") {
                        EditorGUIUtility.systemCopyBuffer = ConvertToValidIdentifier(EditorGUIUtility.systemCopyBuffer);
                    }
                    break;
            }

            Rect controlRect = EditorGUI.PrefixLabel(r, controlId, new GUIContent(label));

            //EditorGUI.RecycledTextEditor editor, int id, Rect position, string text, GUIStyle style, string allowedletters, out bool changed, bool reset, bool multiline, bool passwordField
            object[] parameters = DoTextFieldParameters((TextEditor)recycledEditorField.GetValue(null), controlId, controlRect, text, GUI.skin.textField, null, changed, false, false, false);
            text = (string)doTextFieldMethod.Invoke(null, parameters);

            if(string.IsNullOrEmpty(text) && Event.current.type == EventType.Repaint) {
                Styles.greyItalicLabel.Draw(controlRect, defaultValue, false, false, false, false);
            }

            return text;
        }

        // This is just to make things seem less magic and for IDE compatibility.
        private static object[] DoTextFieldParameters(TextEditor editor, int controlId, Rect position, string text, GUIStyle style, string allowedLetters, bool changed, bool reset, bool multiline, bool passwordField) {
            return new object[] { editor, controlId, position, text, style, allowedLetters, changed, reset, multiline, passwordField};
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