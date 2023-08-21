using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace DeCrawl.Utils
{
    [CustomPropertyDrawer(typeof(WithActionAttribute), true)]
    public class WithActionPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            WithActionAttribute withAction = attribute as WithActionAttribute;

            EditorGUI.BeginProperty(position, label, property);

            Rect propRect = new Rect(position);
            propRect.width *= withAction.split;
            
            EditorGUI.PropertyField(propRect, property, label, true);

            Rect buttonRect = new Rect(position);
            buttonRect.width *= 1 - withAction.split;
            buttonRect.x += position.width * withAction.split;

            if (GUI.Button(buttonRect, new GUIContent(withAction.name, withAction.tooltip))) {
                var so = property.serializedObject;
                
                var methodInfo = so.targetObject.GetType().GetMethod(withAction.callback, withAction.bindingFlags);
                if (methodInfo == null)
                {
                    Debug.LogError($"Could not locate {withAction.callback} on {so.targetObject}");
                }
                else
                {
                    methodInfo.Invoke(so.targetObject, new object[] { });
                }
            }


            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviourEditor : Editor { }

    [CustomEditor(typeof(ScriptableObject), true)]
    public class ScriptableObjectEditr : Editor { }
}
#endif