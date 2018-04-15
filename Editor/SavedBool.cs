using UnityEditor;

public class SavedBool : SavedProperty {
    private bool value;
    public bool Value {
        get { return value; }
        set {
            if(this.value == value) return;
            this.value = value;
            EditorPrefs.SetBool(fullKey, value);
        }
    }

    private SavedBool(string name) : base(name) {
        value = EditorPrefs.GetBool(fullKey);
    }
}
