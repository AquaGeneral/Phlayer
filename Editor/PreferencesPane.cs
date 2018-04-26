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
* - Validate output directory 
* - Output directory needs to allow for '/'
*/

namespace JesseStiller.PhLayerTool {
    public class PreferencesPane {
        private static readonly Type unityPreferencesWindowType = typeof(Editor).Assembly.GetType("UnityEditor.PreferencesWindow");
        private static readonly MethodInfo doTextFieldMethod = typeof(EditorGUI).GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo recycledEditorField = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly int radioButtonsControlHash = "PhLayer.RadioButtons".GetHashCode();
        private static readonly Settings defaultSettings = new Settings();

        private static string generatorPreviewText;
        private static bool previewFoldout = false;
        private static bool expandWindowHeight = false;

        private static class Styles {
            internal static GUIStyle greyItalicLabel, wordWrappedTextArea, previewTextArea, radioButton;
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
                if(previewTextArea == null) {
                    previewTextArea = new GUIStyle(GUI.skin.box);
                    previewTextArea.alignment = TextAnchor.UpperLeft;
                    previewTextArea.font = EditorStyles.miniLabel.font;
                    previewTextArea.fontSize = 10;
                }
                if(radioButton == null) {
                    radioButton = new GUIStyle(EditorStyles.radioButton);
                    radioButton.padding = new RectOffset(radioButton.padding.left, 2, 0, 0);
                }
            }
        }

        private static class Contents {
            internal static readonly GUIContent[] fileNameExtensions = new GUIContent[] { new GUIContent(".cs"), new GUIContent(".g.cs") };
            internal static readonly GUIContent[] curlyBracketPreference = new GUIContent[] { new GUIContent("Same Line"), new GUIContent("New Line") };
            internal static readonly GUIContent[] lineEndings = new GUIContent[] { new GUIContent("Windows-style"), new GUIContent("Unix-style") };
            internal static readonly string[] casing = new string[] {
                "Leave As-Is", "Camel", "Pascal", "Caps Lock", "Caps Lock (Underscored)",
            };
        }

        [PreferenceItem("PhLayer")]
        private static void DrawPreferences() {
            Styles.Initialize();
            PhLayer.InitializeSettings();

            if(string.IsNullOrEmpty(generatorPreviewText)) {
                generatorPreviewText = Generator.GetPreview();
            }

            switch(PhLayer.errorState) {
                case SettingsError.NoDirectory:
                    EditorGUILayout.HelpBox("There is no valid directory named PhLayer - do not rename the directory that PhLayer is contained within.", MessageType.Error);
                    break;
                case SettingsError.NoValidFile:
                    EditorGUILayout.HelpBox("PhLayer somehow couldn't find the main location of its files. Make sure you did not modify PhLayer's code, nor directory names.", MessageType.Error);
                    break;
            }

            EditorGUIUtility.labelWidth = 140f;

            /**
            * File
            */
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("File", EditorStyles.boldLabel);
            PhLayer.settings.className = ValidatedTextField("Class Name*", PhLayer.settings.className, "Layers");
            PhLayer.settings.appendDotGInFileName = RadioButtonsControl(new GUIContent("Filename Extension"), PhLayer.settings.appendDotGInFileName ? 1 : 0, Contents.fileNameExtensions) == 1;
            EditorGUILayout.BeginHorizontal();
            PhLayer.settings.outputDirectory = TextAreaWithDefault("Output Directory*", PhLayer.settings.outputDirectory, "Assets\\");
            if(GUILayout.Button("Browse…", GUILayout.Width(70f), GUILayout.Height(22f))) {
                string chosenPath = EditorUtility.OpenFolderPanel("PhLayer", GetOutputDirectory(), string.Empty);
                if(string.IsNullOrEmpty(chosenPath) == false) PhLayer.settings.outputDirectory = GetLocalPathFromAbsolutePath(chosenPath);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Output Filepath", Generator.GetLocalPath());

            /**
            * Generated Code
            */
            EditorGUILayout.LabelField("Generated Code", EditorStyles.boldLabel);
            PhLayer.settings.classNamespace = ValidatedTextField("Class Namespace", PhLayer.settings.classNamespace);
            PhLayer.settings.casing = (Casing)EditorGUILayout.Popup("Field Casing", (int)PhLayer.settings.casing, Contents.casing);
            PhLayer.settings.indentationStyle = (IndentationStyle)EditorGUILayout.EnumPopup("Indentation Style", PhLayer.settings.indentationStyle);
            PhLayer.settings.lineEndings = (LineEndings)RadioButtonsControl(new GUIContent("Line Endings"), (int)PhLayer.settings.lineEndings, Contents.lineEndings);
            PhLayer.settings.curlyBracketOnNewLine = RadioButtonsControl(new GUIContent("Curly Brackets"), PhLayer.settings.curlyBracketOnNewLine ? 1 : 0, Contents.curlyBracketPreference) == 1;
            PhLayer.settings.skipBuiltinLayers = EditorGUILayout.Toggle("Skip Builtin Layers", PhLayer.settings.skipBuiltinLayers);
            PhLayer.settings.includeHeader = EditorGUILayout.Toggle("Include Header", PhLayer.settings.includeHeader);

            if(EditorGUI.EndChangeCheck()) {
                generatorPreviewText = Generator.GetPreview();
                if(previewFoldout) expandWindowHeight = true;
                PhLayer.SaveSettings();
            }

            using(new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                EditorStyles.centeredGreyMiniLabel.richText = true;
                GUILayout.Label("* Values in <i>italics</i> represent default values", EditorStyles.centeredGreyMiniLabel);
                EditorStyles.centeredGreyMiniLabel.richText = false;
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
                EditorGUI.TextArea(previewTextAreaRect, generatorPreviewText, Styles.previewTextArea);
            }

            using(new EditorGUILayout.HorizontalScope()) {
                if(GUILayout.Button("Force Layers Class Generation", GUILayout.Width(140f), GUILayout.Height(22f))) {
                    Generator.GenerateAndSave();
                }

                GUI.enabled = !PhLayer.settings.Equals(defaultSettings);
                if(GUILayout.Button("Restore Defaults", GUILayout.Width(140f), GUILayout.Height(22f))) {
                    PhLayer.settings = new Settings();
                }
                GUI.enabled = true;
            }

            // Expand the window height so that is shows all of our GUI elements
            if(expandWindowHeight && Event.current.type == EventType.Repaint) {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                EditorWindow editorWindow = EditorWindow.GetWindow(unityPreferencesWindowType);
                editorWindow.position = new Rect(
                    editorWindow.position.x, editorWindow.position.y, editorWindow.position.width, Mathf.Max(editorWindow.position.height, lastRect.y + lastRect.height + 60f)
                );
                expandWindowHeight = false;
            }
        }

        private static string GetOutputDirectory() {
            if(string.IsNullOrEmpty(PhLayer.settings.outputDirectory)) {
                return GetLocalPathFromAbsolutePath(PhLayer.mainDirectory);
            } else {
                return PhLayer.settings.outputDirectory;
            }
        }

        /// <summary>
        /// A radio button control that doesn't cutoff certain characters and with intelligent spacing between options.
        /// </summary>
        /// <returns>The index of the selected radio button.</returns>
        internal static int RadioButtonsControl(GUIContent labelContent, int selectedIndex, GUIContent[] radioButtonOptions) {
            Rect controlRect = EditorGUILayout.GetControlRect();
            Rect toolbarRect = EditorGUI.PrefixLabel(controlRect, labelContent);

            if(radioButtonOptions.Length == 0) return selectedIndex;

            int toolbarOptionsCount = radioButtonOptions.Length;
            float[] widths = new float[toolbarOptionsCount];
            bool useEqualSpacing = true;
            float totalContentsWidths = 0f;
            float maxWidthPerContent = toolbarRect.width / toolbarOptionsCount;

            // Calculate widths of the options and check if the options can be displayed with equal width without anything being cutoff.
            for(int i = 0; i < toolbarOptionsCount; i++) {
                widths[i] = EditorStyles.radioButton.CalcSize(radioButtonOptions[i]).x;
                totalContentsWidths += widths[i];

                // If the width of the GUIContent extends the max width per content while mantaining equal spacing then equal spacing cannot be maintained.
                if(useEqualSpacing && widths[i] > maxWidthPerContent) {
                    useEqualSpacing = false;
                }
            }

            float gapPerOption = (toolbarRect.width - totalContentsWidths) / (toolbarOptionsCount - 1);

            // Find the selected option
            float optionOffset = toolbarRect.x;
            int newSelectedIndex = -1;
            for(int i = 0; i < toolbarOptionsCount; i++) {
                float width = useEqualSpacing ? maxWidthPerContent : gapPerOption + widths[i];
                if(Event.current.mousePosition.x >= optionOffset && Event.current.mousePosition.x <= optionOffset + width) {
                    newSelectedIndex = i;
                }
                optionOffset += width;
            }

            int controlId = GUIUtility.GetControlID(radioButtonsControlHash, FocusType.Passive, controlRect);
            switch(Event.current.GetTypeForControl(controlId)) {
                case EventType.MouseDown:
                    if(toolbarRect.Contains(Event.current.mousePosition) == false) break;
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    break;
                case EventType.MouseUp:
                    if(GUIUtility.hotControl != controlId) break;
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    GUI.changed = true;

                    if(newSelectedIndex == -1) return 0;

                    return newSelectedIndex;
                case EventType.MouseDrag:
                    if(GUIUtility.hotControl == controlId) Event.current.Use();
                    break;
                case EventType.Repaint:
                    float xOffset = toolbarRect.x;
                    for(int i = 0; i < toolbarOptionsCount; i++) {
                        Styles.radioButton.Draw(
                            position: new Rect(xOffset, toolbarRect.y - 1, widths[i], toolbarRect.height),
                            content: radioButtonOptions[i],
                            isHover: i == newSelectedIndex && (GUI.enabled || controlId == GUIUtility.hotControl) && (controlId == GUIUtility.hotControl || GUIUtility.hotControl == 0),
                            isActive: GUIUtility.hotControl == controlId && GUI.enabled,
                            on: i == selectedIndex,
                            hasKeyboardFocus: false);

                        if(useEqualSpacing) {
                            xOffset += maxWidthPerContent;
                        } else {
                            xOffset += gapPerOption + widths[i];
                        }
                    }
                    break;
            }

            return selectedIndex;
        }

        private static readonly int validatedTextFieldId = "PhLayerValidatedTextField".GetHashCode();
        internal static string ValidatedTextField(string label, string text, string defaultValue = "") {
            Rect r = EditorGUILayout.GetControlRect();

            int controlId = GUIUtility.GetControlID(validatedTextFieldId, FocusType.Keyboard, r);
            bool changed = false;
            switch(Event.current.GetTypeForControl(controlId)) {
                case EventType.KeyDown:
                    if(Event.current.character == 0) break; // Allow backspace, delete, etc
                    if(Utilities.IsCharacterValidIdentifier(Event.current.character)) break;
                    Event.current.Use();
                    break;
                case EventType.ExecuteCommand:
                    // HACK: This changes the clipboard value, ideally it shouldn't with more complicated logic, but is it worth it?
                    if(Event.current.commandName == "Paste") {
                        EditorGUIUtility.systemCopyBuffer = Utilities.ConvertToValidIdentifier(EditorGUIUtility.systemCopyBuffer);
                    }
                    break;
            }

            Rect controlRect = EditorGUI.PrefixLabel(r, controlId, new GUIContent(label));

            //EditorGUI.RecycledTextEditor editor, int id, Rect position, string text, GUIStyle style, string allowedletters, out bool changed, bool reset, bool multiline, bool passwordField
            object[] parameters = DoTextFieldParameters((TextEditor)recycledEditorField.GetValue(null), controlId, controlRect, text, GUI.skin.textField, null, changed, false, false, false);
            text = (string)doTextFieldMethod.Invoke(null, parameters);

            if(Event.current.type == EventType.Repaint && string.IsNullOrEmpty(defaultValue) == false && string.IsNullOrEmpty(text)) {
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