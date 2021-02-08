using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JesseStiller.PhlayerTool {
    public class PreferencesPane {
        private static readonly Type unityPreferencesWindowType = typeof(Editor).Assembly.GetType(
            #if UNITY_2018_3_OR_NEWER
            "UnityEditor.SettingsWindow"
            #else
            "UnityEditor.PreferencesWindow"
            #endif
            );
        private static readonly MethodInfo doTextFieldMethod = typeof(EditorGUI).GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly TextEditor recycledEditor = (TextEditor)typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        private static readonly Settings defaultSettings = ScriptableObject.CreateInstance<Settings>();

        private static string generatorPreviewText;
        private static bool previewFoldout = false;
        private static bool expandWindowHeight = false;
        private static Vector2 scrollPosition;

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
            internal static readonly GUIContent[] fileNameExtensions = { new GUIContent(".g.cs"), new GUIContent(".cs") };
            internal static readonly GUIContent[] curlyBracketPreference = { new GUIContent("New line"), new GUIContent("Same line") };
            internal static readonly GUIContent[] lineEndings = { new GUIContent("Windows-style"), new GUIContent("Unix-style") };
            internal static readonly GUIContent[] escapeIdentifierOptions = { new GUIContent("At symbol (@)"), new GUIContent("Underscore (_)") };
            internal static readonly GUIContent localOutputDirectory = new GUIContent("Local Output Directory", 
                "The directory that the generated class will be saved to, which is relative to the current project's \"Assets\" directory.");
            internal static readonly GUIContent[] casing = {
                new GUIContent("Leave as-is"), new GUIContent("Camel"), new GUIContent("Pascal"), new GUIContent("Caps Lock"), new GUIContent("Caps Lock (underscored spaces)")
            };
            internal static readonly GUIContent fieldCasing = new GUIContent("Field Casing");
            internal static readonly GUIContent[] indentationStyles = {
                new GUIContent("1-space"), new GUIContent("2-space"), new GUIContent("3-space"), new GUIContent("4-space"), new GUIContent("Tabs")
            };
        }

        #if UNITY_2018_3_OR_NEWER
        [SettingsProvider]
        private static SettingsProvider SettingsProvider() {
            var provider = new SettingsProvider("Preferences/Phlayer", SettingsScope.User) {
                label = "Phlayer",
                guiHandler = (searchContext) => { 
                    DrawPreferences();
                },
                keywords = new HashSet<string>(new[] { "Physics", "Layer" })
            };

            return provider;
        }
        #endif

        #if !UNITY_2018_3_OR_NEWER
        [PreferenceItem("Phlayer")]
        #endif
        private static void DrawPreferences() {
            Styles.Initialize();
            Phlayer.InitializeSettings();

            switch(Phlayer.errorState) {
                case SettingsError.NoDirectory:
                    EditorGUILayout.HelpBox("There is no valid directory named Phlayer. Ensure Phlayer's main directory has not been modified.", MessageType.Error);
                    return;
                case SettingsError.NoValidFile:
                    EditorGUILayout.HelpBox("Phlayer could not find the main location of its files. Ensure Phlayer's code and directory names have not been modified.", MessageType.Error);
                    return;
            }

            EditorGUIUtility.labelWidth = 142f;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if(string.IsNullOrEmpty(generatorPreviewText)) {
                generatorPreviewText = Generator.GetPreview();
            }
            
            /**
            * File
            */
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("File", EditorStyles.boldLabel);
            Phlayer.settings.className = ValidatedTextField("Class Name", Phlayer.settings.className, true, "Layers");
            Phlayer.settings.appendDotGInFileName = RadioButtonsControl("Filename Extension", Phlayer.settings.appendDotGInFileName ? 0 : 1, Contents.fileNameExtensions) == 0;

            EditorGUILayout.BeginHorizontal();
            Phlayer.settings.localOutputDirectory = DirectoryPathField(Contents.localOutputDirectory, Phlayer.settings.localOutputDirectory);
            if(GUILayout.Button("Browse…", GUILayout.Width(78f), GUILayout.Height(22f))) {
                string chosenPath = EditorUtility.OpenFolderPanel("Browse", GetOutputDirectory(), string.Empty);
                if(string.IsNullOrEmpty(chosenPath) == false) {
                    if(chosenPath.Contains(Application.dataPath) == false) {
                        EditorUtility.DisplayDialog("Browse", "The output directory must be contained within the current Unity project.", "Close");
                    } else {
                        Phlayer.settings.localOutputDirectory = Utilities.GetLocalPathFromAbsolutePath(chosenPath);
                        GUIUtility.keyboardControl = 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Local Output Filepath", Generator.GetLocalPath());

            /**
            * Generated Code
            */
            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            Phlayer.settings.classNamespace = ValidatedTextField("Class Namespace", Phlayer.settings.classNamespace, true);
            Phlayer.settings.casing = (Casing)EditorGUILayout.Popup(Contents.fieldCasing, (int)Phlayer.settings.casing, Contents.casing);
            Phlayer.settings.indentationStyle = (IndentationStyle)EditorGUILayout.Popup(new GUIContent("Indentation Style"), (int)Phlayer.settings.indentationStyle, Contents.indentationStyles);
            Phlayer.settings.windowsStyleLineEndings = RadioButtonsControl("Line Endings", Phlayer.settings.windowsStyleLineEndings ? 0 : 1, Contents.lineEndings) == 0;
            Phlayer.settings.curlyBracketOnNewLine = RadioButtonsControl("Curly Brackets", Phlayer.settings.curlyBracketOnNewLine ? 0 : 1, Contents.curlyBracketPreference) == 0;
            Phlayer.settings.escapeIdentifiersWithAtSymbol = RadioButtonsControl("Escape Character", Phlayer.settings.escapeIdentifiersWithAtSymbol ? 0 : 1, Contents.escapeIdentifierOptions) == 0;
            Phlayer.settings.skipDefaultLayers = EditorGUILayout.Toggle("Skip Default Layers", Phlayer.settings.skipDefaultLayers);
            
            if(EditorGUI.EndChangeCheck()) {
                generatorPreviewText = Generator.GetPreview();
                if(previewFoldout) expandWindowHeight = true;
            }

            EditorGUI.BeginChangeCheck();
#if UNITY_5_5_OR_NEWER
            previewFoldout = EditorGUILayout.Foldout(previewFoldout, "Preview", true);
#else
            previewFoldout = EditorGUILayout.Foldout(previewFoldout, "Preview");
#endif
            if(EditorGUI.EndChangeCheck() && previewFoldout) {
                expandWindowHeight = true;
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

                GUI.enabled = !Phlayer.settings.Equals(defaultSettings);
                if(GUILayout.Button("Restore Defaults", GUILayout.Width(125f), GUILayout.Height(22f)) &&
                    EditorUtility.DisplayDialog("Restore Defaults", "Are you sure you want to restore settings to their defaults?", "Restore Defaults", "Cancel")) {
                    Phlayer.CreateNewSettings();
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

            EditorGUILayout.EndScrollView();
        }

        private static int RadioButtonsControl(string label, int selectedIndex, GUIContent[] options) {
            Rect controlRect = EditorGUILayout.GetControlRect();
            Rect toolbarRect = EditorGUI.PrefixLabel(controlRect, new GUIContent(label));
            return GUI.Toolbar(toolbarRect, selectedIndex, options, Styles.radioButton);
        }

        private static readonly int validatedTextFieldId = "PhlayerValidatedTextField".GetHashCode();
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

        private static readonly int directoryPathFieldId = "PhlayerDirectoryPathField".GetHashCode();
        private static string DirectoryPathField(GUIContent content, string text) {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(29f));
            int controlId = GUIUtility.GetControlID(directoryPathFieldId, FocusType.Keyboard, r);
            Rect controlRect = EditorGUI.PrefixLabel(r, controlId, content);
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

            return DoTextField(controlId, controlRect, text, Styles.wordWrappedTextField, null, false, false, false, false);
        }

        private static string DoTextField(int controlId, Rect rect, string text, GUIStyle style, string allowedLetters, bool changed, bool reset, bool multiline, bool passwordField) {
            object[] parameters = { recycledEditor, controlId, rect, text, style, allowedLetters, changed, reset, multiline, passwordField };
            return (string)doTextFieldMethod.Invoke(null, parameters);
        }

        private static string GetOutputDirectory() {
            if(string.IsNullOrEmpty(Phlayer.settings.localOutputDirectory)) {
                return Phlayer.mainDirectory;
            } else {
                return Phlayer.settings.localOutputDirectory;
            }
        }
    }
}