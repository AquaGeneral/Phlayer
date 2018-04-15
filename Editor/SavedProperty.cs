//internal class SavedProperty {
//    private const string keyPrefix = "JesseStiller.PhLayer/";

//    internal object defaultValue;
//    protected readonly string fullKey;

//    protected SavedProperty(string name) {
//        fullKey = keyPrefix + name;
//    }
//}


using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SavedProperty<T> where T : struct {
    private const string keyPrefix = "JesseStiller.PhLayer/";
    private static readonly Type boolT = typeof(bool);
    private static readonly Type intT = typeof(int);
    private static readonly Type floatT = typeof(float);
    private static readonly Type stringT = typeof(string);

    private readonly string fullKey;
    private readonly T defaultValue;

    private T value;
    public T Value {
        get {
            return value;
        }
        set {
            if(EqualityComparer<T>.Default.Equals(this.value, value)) return;

            this.value = value;

            Type type = typeof(T);
            if(type == boolT) {
                EditorPrefs.SetBool(fullKey, ChangeType<bool>(value));
            } else if(type.IsEnum || type == intT) {
                EditorPrefs.SetInt(fullKey, ChangeType<int>(value));
            } else if(type == floatT) {
                EditorPrefs.SetFloat(fullKey, ChangeType<float>(value));
            } else if(type == stringT) {
                EditorPrefs.SetString(fullKey, ChangeType<string>(value));
            } else {
                Debug.LogError("Mate this type ain't right");
            }
        }
    }

    public SavedProperty(string name, T defaultValue) {
        this.defaultValue = defaultValue;
        fullKey = keyPrefix + name;

        Type type = typeof(T);
        if(type == boolT) {
            value = ChangeType<T>(EditorPrefs.GetBool(fullKey));
        } else if(type == floatT) {
            value = ChangeType<T>(EditorPrefs.GetFloat(fullKey));
        } else if(type.IsEnum || type == intT) {
            value = ChangeType<T>(EditorPrefs.GetInt(fullKey));
        } else if(type == stringT) {
            value = ChangeType<T>(EditorPrefs.GetString(fullKey));
        } else {
            Debug.LogError("Mate this type ain't right");
        }
    }

    private static T1 ChangeType<T1>(object v) {
        return (T1)Convert.ChangeType(v, typeof(T1));
    }
}
