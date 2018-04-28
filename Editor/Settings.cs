#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
using System;
using System.Reflection;

namespace JesseStiller.PhLayerTool {
    [Serializable]
    internal class Settings {
        private static readonly FieldInfo[] fields = typeof(Settings).GetFields(BindingFlags.Instance | BindingFlags.Public);

        /**
        * The initial values also count as the default values (as per pressing "Restore Defaults").
        * The coding convention defaults simply match Microsoft's preferences used in MSDN documentation.
        */
        public Casing casing = Casing.Pascal;
        public bool skipBuiltinLayers = false;
        public string outputDirectory = "";
        public string classNamespace = "";
        public string className = "Layers";
        public bool appendDotGInFileName = true;
        public bool escapeIdentifiersWithAtSymbol = true;
        public bool curlyBracketOnNewLine = true;
        public LineEndings lineEndings = LineEndings.Windows;
        public IndentationStyle indentationStyle = IndentationStyle.Spaces4;

        // Do the rest via reflection so we don't have to update this code every time we update the settings.
        public override bool Equals(object obj) {
            foreach(FieldInfo f in fields) {
                if(f.GetValue(this).Equals(f.GetValue(obj))) continue;
                return false;
            }
            return true;
        }

        public Settings Clone() {
            Settings settings = new Settings();
            foreach(FieldInfo field in fields) {
                field.SetValue(settings, field.GetValue(this));
            }
            return settings;
        }
    }
}