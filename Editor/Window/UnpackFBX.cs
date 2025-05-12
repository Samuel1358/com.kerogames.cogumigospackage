using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CogumigosPackage.Editor.Window
{
#if UNITY_EDITOR
    public class UnpackFBXWindow : EditorWindow
    {
        private UnpackFBXWindowData _data;
        private SerializedObject _serializedObject;

        private SerializedProperty _folderPath;

        private bool _isOverride = false;
        private bool _hasFBX = false;
        private bool _expandCheckList = false;

        private void OnEnable()
        {
            _data = CreateInstance(typeof(UnpackFBXWindowData)) as UnpackFBXWindowData;
            _serializedObject = new SerializedObject(_data);

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

            ChangeGUI();

            Row_Apply();

            GUILayout.Space(4);

            PrefabExportSelection();
        }

        #region // OnGUI

        private void SerializedFields()
        {
            _serializedObject.Update();

            _data.FBX = (GameObject)EditorGUILayout.ObjectField("FBX", _data.FBX, typeof(GameObject), false);
            if (_data.FBX != null)
            {
                string path = AssetDatabase.GetAssetPath(_data.FBX);
                string extention = Path.GetExtension(path).ToLower();
                if (extention != ".fbx")
                {
                    _data.FBX = null;
                    Debug.LogWarning("UnpackFBX: the 'FBX' reference has to be a .fbx asset!");
                }
            }

            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(_folderPath);

            if (GUILayout.Button(". . .", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                string path = EditorUtility.OpenFolderPanel("Unity engine - Unpack FBX", Application.dataPath, "");
                _data.folderPath = StripPath(path);                
            }

            GUILayout.EndHorizontal();

            if (!Directory.Exists(Application.dataPath + _data.folderPath))
            {
                EditorGUILayout.HelpBox($"'{_data.folderPath}' it's not a existent path!", MessageType.Warning);
            }

            _serializedObject.ApplyModifiedProperties();
        }

        private void ChangeGUI()
        {
            if (GUI.changed)
            {
                if (_data.FBX != null)
                {
                    _hasFBX = true;
                    _data.UpdateCheckList(_data.FBX.transform);
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
                if (_data.FBX == null)
                {
                    Debug.LogWarning("No targetParent assigned");
                }
                else
                {
                    string path = Application.dataPath + "/" + _data.folderPath;
                    if (Directory.Exists(path))                    
                    {
                        Unpack(_data.children, _data.objCheckBoxes, _data.staticCheckBoxes, path, _isOverride);
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
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < _data.children.Count; i++)
                    {
                        GUILayout.BeginHorizontal();

                        if (_data.objCheckBoxes[i] = EditorGUILayout.Toggle(_data.children[i].name, _data.objCheckBoxes[i]))
                        {
                            GUILayout.Space(EditorGUIUtility.currentViewWidth * 0.05f);
                            GUILayout.Label("Static");
                            _data.staticCheckBoxes[i] = EditorGUILayout.Toggle(_data.staticCheckBoxes[i]);
                        }
                        else
                            _data.staticCheckBoxes[i] = false;
                        GUILayout.FlexibleSpace();

                        GUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;

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

        #endregion

        public void Unpack(List<GameObject> children, List<bool> exportList, List<bool> staticList, string folderPath, bool isOverride)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (exportList[i])
                {
                    GameObject empty = new GameObject(children[i].name);
                    GameObject aux = Instantiate(children[i]);
                    aux.name = children[i].name;
                    aux.transform.parent = empty.transform;

                    MarkStatic(empty.transform, staticList[i]);

                    TurnIntoPrefab(empty, folderPath, !isOverride);

                    DestroyImmediate(empty);
                }
            }
        }

        private void MarkStatic(Transform transform, bool isStatic)
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.gameObject.isStatic = isStatic;
                    MarkStatic(transform.GetChild(i), isStatic);
                }               
            }
            else
                transform.gameObject.isStatic = isStatic;
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

        internal class UnpackFBXWindowData : ScriptableObject
        {
            public GameObject FBX;
            public string folderPath;
            public List<GameObject> children = new List<GameObject>();
            public List<bool> objCheckBoxes = new List<bool>();
            public List<bool> staticCheckBoxes = new List<bool>();

            public void UpdateCheckList(Transform fbx)
            {
                if (fbx == null)
                    return;

                children.Clear();
                objCheckBoxes.Clear();
                staticCheckBoxes.Clear();

                if (fbx.childCount > 0)
                {
                    for (int i = 0; i < fbx.childCount; i++)
                    {
                        children.Add(fbx.GetChild(i).gameObject);
                        objCheckBoxes.Add(true);
                        staticCheckBoxes.Add(false);
                    }
                }
                else
                {
                    children.Add(fbx.gameObject);
                    objCheckBoxes.Add(true);
                    staticCheckBoxes.Add(false);
                }
            }

            public void SelectAllCheckBoxes()
            {
                for (int i = 0; i < objCheckBoxes.Count; i++)
                {
                    objCheckBoxes[i] = true;
                }
            }

            public void SelectNoneCheckBoxes()
            {
                for (int i = 0; i < objCheckBoxes.Count; i++)
                {
                    objCheckBoxes[i] = false;
                }
            }
        }
    }
#endif
}
