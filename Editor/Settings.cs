using System;

namespace JesseStiller.PhLayerTool {
    [Serializable]
    internal class Settings {
        public Casing casing;
        public bool skipBuiltinLayers;
        public string outputPath;
        public string classNamespace = "";
        public string className = "Layers";
    }
}