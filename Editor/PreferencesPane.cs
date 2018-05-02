using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        private static readonly Type unityPreferencesWindowType = typeof(Editor).Assembly.GetType("UnityEditor.PreferencesWindow");
        private static readonly MethodInfo doTextFieldMethod = typeof(EditorGUI).GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly TextEditor recycledEditor = (TextEditor)typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        private static readonly Settings defaultSettings = new Settings();

        private static string generatorPreviewText;
        private static bool previewFoldout = false;
        private static bool expandWindowHeight = false;

        private static class Styles {
            internal static GUIStyle greyItalicLabel, wordWrappedTextField, previewTextArea, radioButton;
            internal static void Initialize() {
                if(greyItalicLabel == null) {
                    greyItalicLabel = new GUIStyle(GUI.skin.label);
                    greyItalicLabel.padding = GUI.skin.textField.padding;
                    greyItalicLabel.margin = GUI.skin.textField.margin;
                    greyItalicLabel.fontStyle = FontStyle.Italic;
                }
                if(wordWrappedTextField == null) {
                    wordWrappedTextField = new GUIStyle(GUI.skin.textField);
                    wordWrappedTextField.wordWrap = true;
                }
                if(previewTextArea == null) {
                    previewTextArea = new GUIStyle(GUI.skin.box);
                    previewTextArea.alignment = TextAnchor.UpperLeft;
                    previewTextArea.font = EditorStyles.miniLabel.font;
                    previewTextArea.fontSize = 10;
                }
                if(radioButton == null) {
                    radioButton = new GUIStyle(EditorStyles.radioButton);
                    radioButton.padding = new RectOffset(radioButton.padding.left, 0, 2, 0);
                }
            }
        }

        private static class Contents {
            internal static readonly GUIContent[] fileNameExtensions = { new GUIContent(".cs"), new GUIContent(".g.cs") };
            internal static readonly GUIContent[] curlyBracketPreference = { new GUIContent("New line"), new GUIContent("Same line") };
            internal static readonly GUIContent[] lineEndings = { new GUIContent("Windows-style"), new GUIContent("Unix-style") };
            internal static readonly GUIContent[] escapeIdentifierOptions = { new GUIContent("At symbol (@)"), new GUIContent("Underscore (_)") };
            internal static readonly string[] casing = {
                "Leave as-is", "Camel", "Pascal", "Caps Lock", "Caps Lock (underscored spaces)"
            };
            internal static readonly GUIContent[] indentationStyles = {
                new GUIContent("1-space"), new GUIContent("2-space"), new GUIContent("3-space"), new GUIContent("4-space"), new GUIContent("Tabs")
            };
        }

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            Styles.Initialize();
            PhLayer.InitializeSettings();

            switch(PhLayer.errorState) {
                case SettingsError.NoDirectory:
                    EditorGUILayout.HelpBox("There is no valid directory named PhLayer - do not rename the directory that PhLayer is contained within.", MessageType.Error);
                    return;
                case SettingsError.NoValidFile:
                    EditorGUILayout.HelpBox("PhLayer somehow couldn't find the main location of its files. Make sure you did not modify PhLayer's code, nor directory names.", MessageType.Error);
                    return;
            }

            if(string.IsNullOrEmpty(generatorPreviewText)) {
                generatorPreviewText = Generator.GetPreview();
            }

            EditorGUIUtility.labelWidth = 140f;

            /**
            * File
            */
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("File", EditorStyles.boldLabel);
            PhLayer.settings.className = ValidatedTextField("Class Name", PhLayer.settings.className, true, "Layers");
            PhLayer.settings.appendDotGInFileName = RadioButtonsControl("Filename Extension", PhLayer.settings.appendDotGInFileName ? 1 : 0, Contents.fileNameExtensions) == 1;
            EditorGUILayout.BeginHorizontal();
            PhLayer.settings.outputDirectory = DirectoryPathField("Output Directory", PhLayer.settings.outputDirectory);
            if(GUILayout.Button("Browse…", GUILayout.Width(80f), GUILayout.Height(22f))) {
                string chosenPath = EditorUtility.OpenFolderPanel("Browse", GetOutputDirectory(), string.Empty);
                if(string.IsNullOrEmpty(chosenPath) == false) {
                    PhLayer.settings.outputDirectory = GetLocalPathFromAbsolutePath(chosenPath);
                    GUIUtility.keyboardControl = 0;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Output Filepath", Generator.GetLocalPath());

            /**
            * Generated Code
            */
            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            PhLayer.settings.classNamespace = ValidatedTextField("Class Namespace", PhLayer.settings.classNamespace, true);
            PhLayer.settings.casing = (Casing)EditorGUILayout.Popup("Field Casing", (int)PhLayer.settings.casing, Contents.casing);
            PhLayer.settings.indentationStyle = (IndentationStyle)EditorGUILayout.Popup(new GUIContent("Indentation Style"), (int)PhLayer.settings.indentationStyle, Contents.indentationStyles);
            PhLayer.settings.lineEndings = (LineEndings)RadioButtonsControl("Line Endings", (int)PhLayer.settings.lineEndings, Contents.lineEndings);
            PhLayer.settings.curlyBracketOnNewLine = RadioButtonsControl("Curly Brackets", PhLayer.settings.curlyBracketOnNewLine ? 0 : 1, Contents.curlyBracketPreference) == 0;
            PhLayer.settings.escapeIdentifiersWithAtSymbol = RadioButtonsControl("Escape Character", PhLayer.settings.escapeIdentifiersWithAtSymbol ? 0 : 1, Contents.escapeIdentifierOptions) == 0;
            PhLayer.settings.skipDefaultLayers = EditorGUILayout.Toggle("Skip Default Layers", PhLayer.settings.skipDefaultLayers);
            
            if(EditorGUI.EndChangeCheck()) {
                generatorPreviewText = Generator.GetPreview();
                if(previewFoldout) expandWindowHeight = true;
                PhLayer.SaveSettings();
            }
            
            using(EditorGUI.ChangeCheckScope change = new EditorGUI.ChangeCheckScope()) {
                previewFoldout = EditorGUILayout.Foldout(previewFoldout, "Preview", true);
                if(change.changed && previewFoldout) {
                    expandWindowHeight = true;
                }
            }
            if(previewFoldout) {
                Rect previewTextAreaRect = EditorGUILayout.GetControlRect(
                    GUILayout.ExpandWidth(true), GUILayout.Height(Styles.previewTextArea.CalcSize(new GUIContent(generatorPreviewText)).y));
                GUI.Label(previewTextAreaRect, generatorPreviewText, Styles.previewTextArea);
            }

            using(new EditorGUILayout.HorizontalScope()) {
                if(GUILayout.Button("Generate Script", GUILayout.Width(125f), GUILayout.Height(22f))) {
                    Generator.GenerateAndSave();
                }

                GUI.enabled = !PhLayer.settings.Equals(defaultSettings);
                if(GUILayout.Button("Restore Defaults", GUILayout.Width(125f), GUILayout.Height(22f))) {
                    PhLayer.settings = new Settings();
                    PhLayer.SaveSettings();
                    generatorPreviewText = Generator.GetPreview();
                    GUIUtility.keyboardControl = 0;
                }
                GUI.enabled = true;
            }

            // Expand the window height so that is shows all of our GUI elements
            if(expandWindowHeight && Event.current.type == EventType.Repaint) {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                EditorWindow editorWindow = EditorWindow.GetWindow(unityPreferencesWindowType);
                editorWindow.position = new Rect(
                    editorWindow.position.x, editorWindow.position.y, editorWindow.position.width, Mathf.Max(editorWindow.position.height, lastRect.y + lastRect.height + 55f)
                );
                expandWindowHeight = false;
            }
        }

        private static int RadioButtonsControl(string label, int selectedIndex, GUIContent[] options) {
            Rect controlRect = EditorGUILayout.GetControlRect();
            Rect toolbarRect = EditorGUI.PrefixLabel(controlRect, new GUIContent(label));
            return GUI.Toolbar(toolbarRect, selectedIndex, options, Styles.radioButton);
        }

        private static readonly int validatedTextFieldId = "PhLayerValidatedTextField".GetHashCode();
        private static string ValidatedTextField(string label, string text, bool allowDots, string defaultValue = "") {
            Rect r = EditorGUILayout.GetControlRect();
            int controlId = GUIUtility.GetControlID(validatedTextFieldId, FocusType.Keyboard, r);
            Rect controlRect = EditorGUI.PrefixLabel(r, controlId, new GUIContent(label));
            Event current = Event.current;
            if(GUIUtility.keyboardControl == controlId) {
                switch(current.GetTypeForControl(controlId)) {
                    case EventType.KeyDown:
                        if(current.character == 0) break; // Allow backspace, delete, etc
                        if(Utilities.IsCharValidForIdentifier(current.character)) break;
                        if(allowDots && current.character == '.') break;
                        current.Use();
                        break;
                    case EventType.ExecuteCommand:
                        // HACK: This changes the clipboard value, ideally it shouldn't with more complicated logic, but is it worth it?
                        if(current.commandName == "Paste") {
                            EditorGUIUtility.systemCopyBuffer = Utilities.ConvertToValidIdentifier(EditorGUIUtility.systemCopyBuffer);
                        }
                        break;
                }
            }

            text = DoTextField(controlId, controlRect, text, GUI.skin.textField, null, false, false, false, false);

            if(current.type == EventType.Repaint && string.IsNullOrEmpty(defaultValue) == false && string.IsNullOrEmpty(text)) {
                Styles.greyItalicLabel.Draw(controlRect, defaultValue, false, false, false, false);
            }

            return text;
        }

        private static readonly int directoryPathFieldId = "PhLayerDirectoryPathField".GetHashCode();
        private static string DirectoryPathField(string label, string text) {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(29f));
            int controlId = GUIUtility.GetControlID(directoryPathFieldId, FocusType.Keyboard, r);
            Rect controlRect = EditorGUI.PrefixLabel(r, controlId, new GUIContent(label));
            Event current = Event.current;
            if(GUIUtility.keyboardControl == controlId) {
                switch(current.GetTypeForControl(controlId)) {
                    case EventType.KeyDown:
                        if(current.character == 0) break; // Allow backspace, delete, etc
                        if(Utilities.IsDirectoryPathCharacterValid(current.character)) break;

                        current.Use();

                        break;
                    case EventType.ExecuteCommand:
                        // HACK: This changes the clipboard value, ideally it shouldn't with more complicated logic, but is it worth it?
                        if(current.commandName == "Paste") {
                            EditorGUIUtility.systemCopyBuffer = Utilities.ConvertToValidDirectoryPath(EditorGUIUtility.systemCopyBuffer);
                        }
                        break;
                }
            }

            text = DoTextField(controlId, controlRect, text, Styles.wordWrappedTextField, null, false, false, false, false);

            return text;
        }

        private static string DoTextField(int controlId, Rect rect, string text, GUIStyle style, string allowedLetters, bool changed, bool reset, bool multiline, bool passwordField) {
            object[] parameters = { recycledEditor, controlId, rect, text, style, allowedLetters, changed, reset, multiline, passwordField };
            return (string)doTextFieldMethod.Invoke(null, parameters);
        }

        private static string GetOutputDirectory() {
            if(string.IsNullOrEmpty(PhLayer.settings.outputDirectory)) {
                return GetLocalPathFromAbsolutePath(PhLayer.mainDirectory);
            } else {
                return PhLayer.settings.outputDirectory;
            }
        }

        private static string GetLocalPathFromAbsolutePath(string absolutePath) {
            int indexOfAssets = absolutePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

            if(indexOfAssets == -1) {
                throw new ArgumentException("The 'assetsPath' parameter must contain 'Assets/'");
            }
            return absolutePath.Remove(0, indexOfAssets);
        }
    }
}