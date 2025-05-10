using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CogumigosPackage.Editor.Inspector
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MultiPrefab))]
    public class MultiPrefebEditor : UnityEditor.Editor
    {
        private SerializedProperty _script;

        private MultiPrefab _multiPrefab;
        private SerializedObject _serializedObject;

        private SerializedProperty _targetParent;
        private SerializedProperty _variants;

        private MultiPrefebInspector_DropdownDisplay _dropdownDisplay;
        private MultiPrefebInspector_ButtonDisplay _buttonDisplay;

        private UnityEditor.Editor[] _displays;

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

            _displays = new UnityEditor.Editor[] { _dropdownDisplay, _buttonDisplay };
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

        internal class MultiPrefebInspector_DropdownDisplay : UnityEditor.Editor
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

        internal class MultiPrefebInspector_ButtonDisplay : UnityEditor.Editor
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
}
