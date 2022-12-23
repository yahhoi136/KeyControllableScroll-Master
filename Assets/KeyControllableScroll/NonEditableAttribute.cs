using UnityEngine;

namespace UnityEditor.UI
{
    /// <summary>
    /// インスペクタで編集不可
    /// </summary>
    public class NonEditableAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(NonEditableAttribute))]
    public class NonEditableAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
