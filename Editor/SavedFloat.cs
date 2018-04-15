using UnityEditor;

public class SavedFloat : SavedProperty {
    private float value;
    public float Value {
        get { return value; }
        set {
            if(this.value == value) return;
            this.value = value;
            EditorPrefs.SetFloat(fullKey, value);
        }
    }

    private SavedFloat(string name, float defaultValue) : base(name) {
        value = EditorPrefs.GetFloat(fullKey, defaultValue);
    }
}
