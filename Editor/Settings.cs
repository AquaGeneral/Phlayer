using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JesseStiller.PhLayerTool {
    internal static class Settings {
        public static readonly SavedProperty<Casing> casing = new SavedProperty<Casing>("casing", Casing.Camel);
        public static readonly SavedProperty<bool> skipBuiltinLayers = new SavedProperty<bool>("skipBuiltinLayers", false);
    }
}