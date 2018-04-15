using UnityEditor;

public class SavedInt : SavedProperty {
    private int value;
    public int Value {
        get { return value; }
        set {
            if(this.value == value) return;
            this.value = value;
            EditorPrefs.SetInt(fullKey, value);
        }
    }

    private SavedInt(string name, int defaultValue) : base(name) {
        value = EditorPrefs.GetInt(fullKey, defaultValue);
    }
}
