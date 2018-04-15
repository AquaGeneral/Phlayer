using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JesseStiller.PhLayerTool {
    internal class Settings {


        public static readonly SavedProperty<Casing> casing = new SavedProperty<Casing>("casing", Casing.Camel);
        public static readonly SavedProperty<bool> skipBuiltinLayers = new SavedProperty<bool>("skipBuiltinLayers", false);
        public static readonly SavedProperty<string> saveLocation = new SavedProperty<string>("saveLocation", "");
        public static readonly SavedProperty<string> classNameSpace = new SavedProperty<string>("classNameSpace", "");
        public static readonly SavedProperty<string> className = new SavedProperty<string>("className", "Layers");
    }
}