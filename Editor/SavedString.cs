using UnityEditor;

public class SavedString : SavedProperty {
    private string value;
    public string Value {
        get { return value; }
        set {
            if(this.value == value) return;
            this.value = value;
            EditorPrefs.SetString(fullKey, value);
        }
    }

    private SavedString(string name) : base(name) {
        value = EditorPrefs.GetString(fullKey);
    }
}
