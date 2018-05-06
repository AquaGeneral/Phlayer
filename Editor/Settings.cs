using System;
using System.Reflection;
using UnityEngine;

namespace JesseStiller.PhlayerTool {
    [Serializable]
    internal class Settings : ScriptableObject {
        private static readonly FieldInfo[] fields = typeof(Settings).GetFields(BindingFlags.Instance | BindingFlags.Public);

        /**
        * The initial values also count as the default values (as per pressing "Restore Defaults").
        * The coding convention defaults simply match Microsoft's preferences used in MSDN documentation.
        */
        public Casing casing = Casing.Pascal;
        public bool skipDefaultLayers = false;
        public string localOutputDirectory = "";
        public string classNamespace = "";
        public string className = "Layers";
        public bool appendDotGInFileName = true;
        public bool escapeIdentifiersWithAtSymbol = true;
        public bool curlyBracketOnNewLine = true;
        public bool windowsStyleLineEndings = true;
        public IndentationStyle indentationStyle = IndentationStyle.FourSpaces;

        // Do the rest via reflection so we don't have to update this code every time we update the settings.
        public override bool Equals(object obj) {
            foreach(FieldInfo f in fields) {
                if(f.GetValue(this).Equals(f.GetValue(obj))) continue;
                return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public Settings Clone() {
            Settings settings = ScriptableObject.CreateInstance<Settings>();
            foreach(FieldInfo field in fields) {
                field.SetValue(settings, field.GetValue(this));
            }
            return settings;
        }
    }
}