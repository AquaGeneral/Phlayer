using System;

namespace JesseStiller.PhLayerTool {
    [Serializable]
    internal class Settings {
        public Casing casing;
        public bool skipBuiltinLayers;
        public string outputDirectory;
        public string classNamespace = "";
        public string className = "Layers";
        public bool appendDotGInFileName = true;
        // Yes the following could just be bools but listing the options explicitly in the UI makes it more user-friendly.
        public bool curlyBracketOnNewLine = true;
        public LineEndings lineEndings = LineEndings.Windows; 
    }
}