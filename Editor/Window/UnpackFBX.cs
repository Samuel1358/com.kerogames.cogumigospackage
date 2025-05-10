using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CogumigosPackage.Editor.Window
{
#if UNITY_EDITOR
    public class UnpackFBXWindow : EditorWindow
    {
        private UnpackFBX _functional;
        private UnpackFBXWindowData _data;
        private SerializedObject _serializedObject;

        private SerializedProperty _targetParent;
        private SerializedProperty _folderPath;

        private bool _isOverride = false;
        private bool _hasFBX = false;
        private bool _expandCheckList = false;

        private void OnEnable()
        {
            _functional = CreateInstance(typeof(UnpackFBX)) as UnpackFBX;
            _data = CreateInstance(typeof(UnpackFBXWindowData)) as UnpackFBXWindowData;
            _serializedObject = new SerializedObject(_data);

            _targetParent = _serializedObject.FindProperty("targetParent");
            _folderPath = _serializedObject.FindProperty("folderPath");
        }

        [MenuItem("Tools/UnpackFBX")]
        public static void ShowWindow()
        {
            GetWindow<UnpackFBXWindow>("UnpackFBX");
        }

        private void OnGUI()
        {
            SerializedFields();

            Row_Apply();

            GUILayout.Space(4);

            PrefabExportSelection();
        }

        private string StripPath(string path)
        {
            if (Application.dataPath.Length > path.Length)
                return "";


            for (int i = 0; i < Application.dataPath.Length; i++)
            {
                if (Application.dataPath[i] != path[i])
                {
                    return "";
                }
            }

            string newPath = "";

            for (int i = Application.dataPath.Length; i < path.Length; i++)
            {
                newPath += path[i];
            }

            return newPath;
        }

        #region // OnGUIMethods

        private void SerializedFields()
        {
            _serializedObject.Update();

            if (_targetParent != null)
                EditorGUILayout.PropertyField(_targetParent);

            GUILayout.BeginHorizontal();

            if (_folderPath != null)
                EditorGUILayout.PropertyField(_folderPath);

            if (GUILayout.Button(". . .", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                string path = EditorUtility.OpenFolderPanel("Unity engine - Unpack FBX", Application.dataPath, "");
                _data.folderPath = StripPath(path);
            }

            GUILayout.EndHorizontal();

            if (_serializedObject.ApplyModifiedProperties())
            {
                if (_data.targetParent != null)
                {
                    _hasFBX = true;
                    _data.UpdateCheckList(_data.targetParent);
                }
                else
                {
                    _hasFBX = false;
                    _expandCheckList = false;
                }
            }
        }

        private void Row_Apply()
        {
            GUILayout.BeginHorizontal();

            _isOverride = EditorGUILayout.Toggle("Override", _isOverride);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                if (_data.targetParent == null)
                {
                    Debug.LogWarning("No targetParent assigned");
                }
                else
                {

                    string path = Application.dataPath + "/" + _data.folderPath;
                    if (!Directory.Exists(path))
                    {
                        Debug.LogWarning($"Folder {path} don't exists!");
                    }
                    else
                    {

                        _functional.Unpack(_data.children, _data.checkBoxes, path, _isOverride);

                    }

                }
            }

            GUILayout.EndHorizontal();
        }

        private void PrefabExportSelection()
        {
            if (_hasFBX)
            {
                _expandCheckList = EditorGUILayout.Foldout(_expandCheckList, "Prefabs export selection", true);

                if (_expandCheckList)
                {
                    for (int i = 0; i < _data.children.Count; i++)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(EditorGUIUtility.currentViewWidth * 0.05f);
                        _data.checkBoxes[i] = EditorGUILayout.Toggle(_data.children[i].name, _data.checkBoxes[i]);

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.currentViewWidth * 0.05f);

                    if (GUILayout.Button("All", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.05f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        _data.SelectAllCheckBoxes();
                    }

                    GUILayout.Space(8);

                    if (GUILayout.Button("None", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.05f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        _data.SelectNoneCheckBoxes();
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
        }

        #endregion

        internal class UnpackFBXWindowData : ScriptableObject
        {
            public Transform targetParent;
            public string folderPath;
            public List<GameObject> children = new List<GameObject>();
            public List<bool> checkBoxes = new List<bool>();

            public void UpdateCheckList(Transform targetParent)
            {
                if (targetParent == null)
                    return;

                children.Clear();
                checkBoxes.Clear();
                for (int i = 0; i < targetParent.childCount; i++)
                {
                    children.Add(targetParent.GetChild(i).gameObject);

                    checkBoxes.Add(true);
                }
            }

            public void SelectAllCheckBoxes()
            {
                for (int i = 0; i < checkBoxes.Count; i++)
                {
                    checkBoxes[i] = true;
                }
            }

            public void SelectNoneCheckBoxes()
            {
                for (int i = 0; i < checkBoxes.Count; i++)
                {
                    checkBoxes[i] = false;
                }
            }
        }
    }

    public class UnpackFBX : ScriptableObject
    {
        public void Unpack(List<GameObject> children, List<bool> exportList, string folderPath, bool isOverride)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (exportList[i])
                {
                    GameObject empty = new GameObject(children[i].name);
                    GameObject aux = Instantiate(children[i]);
                    aux.name = children[i].name;
                    aux.transform.parent = empty.transform;

                    TurnIntoPrefab(empty, folderPath, !isOverride);

                    DestroyImmediate(empty);
                }
            }
        }

        private void TurnIntoPrefab(GameObject obj, string path, bool uniquePath)
        {
            if (obj == null) return;

            if (!Directory.Exists(path))
            {
                Debug.LogWarning($"Folder {path} don't finded!");
                return;
            }

            if (uniquePath)
            {
                path = UniquePath(path + $"/{obj.name}", ".prefab");
                path += ".prefab";
            }
            else
            {
                path += $"/{obj.name}.prefab";
            }

            PrefabUtility.SaveAsPrefabAsset(obj, path);
        }

        private string UniquePath(string path, string additional)
        {
            while (File.Exists(path + additional))
            {
                if (int.TryParse(path[path.Length - 1].ToString(), out int id))
                {
                    int l = 1;
                    for (int i = 0; int.TryParse(path[path.Length - l].ToString(), out i); l++) { id = i; }

                    path = path.Remove(path.Length - (l - 1));
                    path += id + 1;
                }
                else
                {
                    path += " 1";
                }
            }

            return path;
        }
    }
#endif
}
