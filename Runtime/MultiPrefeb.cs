using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class MultiPrefab : MonoBehaviour
{
    [SerializeField] public Transform targetParent;
    [SerializeField] public List<GameObject> variants = new List<GameObject>();
    [SerializeField] public GameObject registerObject;
    [SerializeField] public int selectedIndex = 0;

    // Inspector
    public int _selectedDisplayIndex = 0;

    // Public Methods
    public void Apply()
    {
        if (targetParent == null)
            return;

        if (variants.Count == 0)
            return;

        GameObject variant = variants[selectedIndex];
        if (variant == null)
        {
            ResetPrefabChildren();
            return;
        }

        ResetPrefabChildren();

        ResetPrefabComponents();

        GameObject aux = Instantiate(variant, targetParent);

        TransferePrefabChildren(aux);

        TransferePrefabComponents(aux);


        DestroyImmediate(aux);
    }

    // Private Methods
    private void ResetPrefabChildren()
    {
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < targetParent.childCount; i++)
        {
            children.Add(targetParent.GetChild(i).gameObject);
        }

        foreach (GameObject child in children)
        {
            DestroyImmediate(child);
        }
    }

    private void ResetPrefabComponents()
    {
        List<Component> components = new List<Component>();
        for (int i = 0; i < targetParent.gameObject.GetComponentCount(); i++)
        {
            components.Add(targetParent.gameObject.GetComponentAtIndex(i));
        }

        foreach (Component component in components)
        {
            if (component.GetType() != typeof(Transform) && component.GetType() != typeof(MultiPrefab))
            {
                DestroyImmediate(component);
            }
        }
    }

    private void TransferePrefabChildren(GameObject aux)
    {
        List<Transform> auxChildren = new List<Transform>();
        for (int i = 0; i < aux.transform.childCount; i++)
        {
            auxChildren.Add(aux.transform.GetChild(i));
        }

        foreach (Transform child in auxChildren)
        {
            child.transform.SetParent(targetParent);
        }
    }

    private void TransferePrefabComponents(GameObject aux)
    {
        List<Component> auxCoponents = new List<Component>();
        for (int i = 0; i < aux.GetComponentCount(); i++)
        {
            auxCoponents.Add(aux.GetComponentAtIndex(i));
        }

        foreach (Component component in auxCoponents)
        {
            if (targetParent.GetComponent(component.GetType()) == null)
            {
                System.Type type = component.GetType();
                var copy = targetParent.gameObject.AddComponent(type);

                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                FieldInfo[] fields = type.GetFields(flags);
                foreach (FieldInfo field in fields)
                {
                    field.SetValue(copy, field.GetValue(component));
                }

                PropertyInfo[] properties = type.GetProperties(flags);
                foreach (PropertyInfo property in properties)
                {
#pragma warning disable CS0168 // A variável foi declarada, mas nunca foi usada
                    try
                    {
                        property.SetValue(copy, property.GetValue(component));
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MultiPrefab))]
public class MultiPrefebInspector : Editor
{
    private SerializedProperty _script;

    private MultiPrefab _multiPrefab;
    private SerializedObject _serializedObject;

    private SerializedProperty _targetParent;
    private SerializedProperty _variants;

    private MultiPrefebInspector_DropdownDisplay _dropdownDisplay;
    private MultiPrefebInspector_ButtonDisplay _buttonDisplay;

    private Editor[] _displays;

    private string[] _names = new string[] { "Dropdown", "Buttons" };
    

    private void OnEnable()
    {
        _script = serializedObject.FindProperty("m_Script");

        _multiPrefab = target as MultiPrefab;
        _serializedObject = new SerializedObject(_multiPrefab);

        _targetParent = _serializedObject.FindProperty("targetParent");
        _variants = _serializedObject.FindProperty("variants");

        _dropdownDisplay = CreateInstance(typeof(MultiPrefebInspector_DropdownDisplay)) as MultiPrefebInspector_DropdownDisplay;
        _buttonDisplay = CreateInstance(typeof(MultiPrefebInspector_ButtonDisplay)) as MultiPrefebInspector_ButtonDisplay;

        _dropdownDisplay.Setup(_multiPrefab);
        _buttonDisplay.Setup(_multiPrefab);

        _displays = new Editor[] { _dropdownDisplay, _buttonDisplay};
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_script, true);
        EditorGUI.EndDisabledGroup();

        SelectionMode();

        GUILayout.Space(4);

        SerializedFields();

        GUILayout.Space(4);

        _displays[_multiPrefab._selectedDisplayIndex].OnInspectorGUI();
    }

    private void SelectionMode()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Selection mode:");
        GUICustomElements.FlexibleSelectionGrid(ref _multiPrefab._selectedDisplayIndex, _names);

        GUILayout.EndHorizontal();
    }

    private void SerializedFields()
    {
        _serializedObject.Update();

        EditorGUILayout.PropertyField(_targetParent);

        if (_multiPrefab.targetParent == null)
            EditorGUILayout.HelpBox("TargetParent need to be assigned!", MessageType.Warning);

        EditorGUILayout.PropertyField(_variants);

        _serializedObject.ApplyModifiedProperties();
    }

    #region // Displays

    internal class MultiPrefebInspector_DropdownDisplay : Editor
    {
        private MultiPrefab _multiPrefab;

        private int _currentIndex = 0;

        public void Setup(MultiPrefab multiPrefab)
        {
            _multiPrefab = multiPrefab;
        }

        public override void OnInspectorGUI()
        {
            VariantsShiftDropdown();

            GUIChanged();
        }

        #region // OnInspectorGUI       

        private void VariantsShiftDropdown()
        {
            if (_multiPrefab.variants.Count > 0)
            {
                _currentIndex = EditorGUILayout.Popup("Prefab", _multiPrefab.selectedIndex, VarientsNames(_multiPrefab.variants));
            }
        }

        private void GUIChanged()
        {
            if (GUI.changed)
            {
                if (_multiPrefab.selectedIndex != _currentIndex)
                {
                    _multiPrefab.selectedIndex = _currentIndex;

                    if (_multiPrefab.targetParent != null)
                    {
                        _multiPrefab.Apply();
                    }
                }
                /*else if (_multiPrefab.selectedIndex == 0)
                {
                    ApplyVariantSelected();
                }*/
            }
        }

        #endregion

        private string[] VarientsNames(List<GameObject> variants)
        {
            List<string> list = new List<string>();

            foreach (GameObject variant in variants)
            {
                if (variant != null)
                    list.Add(variant.name);
                else list.Add("None");
            }

            return list.ToArray();
        }
    }

    internal class MultiPrefebInspector_ButtonDisplay : Editor
    {
        private MultiPrefab _multiPrefab;


        private bool changedSelected;

        public void Setup(MultiPrefab multiPrefab)
        {
            _multiPrefab = multiPrefab;
        }

        public override void OnInspectorGUI()
        {           
            GUICustomElements.FlexibleSelectionGrid(ref _multiPrefab.selectedIndex, _multiPrefab.variants, out changedSelected);

            if (changedSelected)
            {
                if (_multiPrefab.targetParent != null)
                    _multiPrefab.Apply();
            }
        }
    }

    #endregion
}
#endif
