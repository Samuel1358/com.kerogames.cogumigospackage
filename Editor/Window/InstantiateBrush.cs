using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CogumigosPackage.Editor.Window
{
#if UNITY_EDITOR
    public class InstantiateBrushWindow : EditorWindow
    {
        private InstantiateBrushData _data;
        private SerializedObject _serializedObject;

        #region // Settings

        private SerializedProperty _disabledCursorColor;
        private SerializedProperty _defaultCursorColor;
        private SerializedProperty _cursorThickness;

        private bool _settingsFoldout = false;

        #endregion

        #region // Parameters

        // Header
        private SerializedProperty _parentContainer;
        private SerializedProperty _targetLayers;
        private SerializedProperty _brushType;

        // Brush size
        private SerializedProperty _brushSize;

        // Density
        private SerializedProperty _density;

        // Spread
        private SerializedProperty _instanceDistance;

        // Object variables
        private SerializedProperty _randomObject;
        private SerializedProperty _randomRotationY;
        private SerializedProperty _randomScale;

        private string[] _instantiateModes = { "Prefab", "Clone" };
        private int _instantiateModesIndex = 0;

        // Footer      
        private SerializedProperty _objects;
        public int _selectedIndex = 0;

        #endregion

        private RaycastHit _hit;
        private Vector3? _lastHitPoint;
        private bool _isPainting;
        private Vector3? _lastPaintedPoint;

        private Color _handlesDefaultColor;

        private void OnEnable()
        {
            _data = CreateInstance(typeof(InstantiateBrushData)) as InstantiateBrushData;
            _serializedObject = new SerializedObject(_data);

            #region // Settings

            _disabledCursorColor = _serializedObject.FindProperty("disabledCursorColor");
            _defaultCursorColor = _serializedObject.FindProperty("defaultCursorColor");
            _cursorThickness = _serializedObject.FindProperty("cursorThickness");

            #endregion

            #region // Parameters

            // Header
            _parentContainer = _serializedObject.FindProperty("parentContainer");
            _targetLayers = _serializedObject.FindProperty("targetLayers");
            _brushType = _serializedObject.FindProperty("brushType");

            // Brush size
            _brushSize = _serializedObject.FindProperty("brushSize");

            // Density
            _density = _serializedObject.FindProperty("density");

            // Spread
            _instanceDistance = _serializedObject.FindProperty("instanceDistance");

            // Object variables
            _randomObject = _serializedObject.FindProperty("randomObject");
            _randomRotationY = _serializedObject.FindProperty("randomRotationY");
            _randomScale = _serializedObject.FindProperty("randomScale");

            // Footer          
            _objects = _serializedObject.FindProperty("objects");

            #endregion

            _handlesDefaultColor = Handles.color;
        }

        private void OnBecameVisible()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnBecameInvisible()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        [MenuItem("Tools/InstantiateBrush")]
        public static void ShowWindow()
        {
            GetWindow<InstantiateBrushWindow>("Instantiate Brush");
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            SettingsFoldout();

            GUILayout.Label("Parameters", EditorStyles.boldLabel);

            SerializedFields();

            _serializedObject.ApplyModifiedProperties();

            GUICustomElements.FlexibleSelectionGrid(ref _selectedIndex, _data.objects);
        }

        #region // OnInspectorGUI

        private void SettingsFoldout()
        {          
            _settingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_settingsFoldout, new GUIContent("Settings", "'Brush' cursor settings"));

            if (_settingsFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_disabledCursorColor);
                EditorGUILayout.Space(1);
                EditorGUILayout.PropertyField(_defaultCursorColor);
                EditorGUILayout.Space(1);
                EditorGUILayout.PropertyField(_cursorThickness);
                EditorGUILayout.Space(1);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(1);
        }

        private void SerializedFields()
        {
            // Header
            EditorGUILayout.PropertyField(_parentContainer);
            EditorGUILayout.Space(1);

            if (_data.parentContainer == null)
                EditorGUILayout.HelpBox("'Parent Container' need to be assigned!", MessageType.Error);

            EditorGUILayout.PropertyField(_targetLayers);
            EditorGUILayout.Space(1);
            EditorGUILayout.PropertyField(_brushType);
            _data.brushType = (InstantiateBrushData.BrushType)_brushType.enumValueIndex;
            EditorGUILayout.Space(1);

            #region // Brush size

            if (_data.brushType == InstantiateBrushData.BrushType.Eraser || _data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_brushSize);
                EditorGUILayout.Space(1);
            }

            #endregion

            #region // Density

            if (_data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_density);
                EditorGUILayout.Space(1);
            }

            #endregion

            #region // Spread

            if (_data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_instanceDistance);
                EditorGUILayout.Space(1);
            }

            #endregion

            #region // Object variables

            if (_data.brushType == InstantiateBrushData.BrushType.Stamp || _data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_randomObject);
                EditorGUILayout.Space(1);
                EditorGUILayout.PropertyField(_randomRotationY);
                EditorGUILayout.Space(1);
                EditorGUILayout.PropertyField(_randomScale);
                EditorGUILayout.Space(1);

                if ((_data.objects.Count > 0) ? PrefabUtility.IsPartOfPrefabAsset(_data.objects[_selectedIndex]) : false)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent("Instantiate mode:", "If the reference of the object to be instantiated is a prefab may choose instantiate it as a prefab or a clone of the GameObject"));
                    GUICustomElements.FlexibleSelectionGrid(ref _instantiateModesIndex, _instantiateModes);
                    EditorGUILayout.Space(1);

                    GUILayout.EndHorizontal();
                }
                else
                    _instantiateModesIndex = 1;
                
            }           

            #endregion

            // Footer
            EditorGUILayout.PropertyField(_objects);

            if (_data.objects.Count <= 0)
            {
                EditorGUILayout.HelpBox("No object assigned in the list!", MessageType.Warning);
                if (_data.brushType == InstantiateBrushData.BrushType.Eraser)
                    EditorGUILayout.HelpBox("The eraser don't work while 'Objects' list is empty!", MessageType.Warning);
            }
        }

        #endregion

        private void OnSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;

            SceneView currentView = SceneView.currentDrawingSceneView;
            if (currentView == null || currentView.camera == null)
                return;

            Vector2 mousePosition = currentEvent.mousePosition;
            mousePosition.y = currentView.camera.pixelHeight - mousePosition.y;
            Ray ray = currentView.camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out _hit, Mathf.Infinity, _data.targetLayers))
            {                
                _lastHitPoint = _hit.point;

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    _isPainting = true;
                    Paint();
                    currentEvent.Use();
                }

                if (_data.brushType == InstantiateBrushData.BrushType.Eraser || _data.brushType == InstantiateBrushData.BrushType.Spray)
                {
                    if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && _isPainting)
                    {
                        Paint();
                        currentEvent.Use();
                    }

                    if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                    {
                        _isPainting = false;
                        currentEvent.Use();
                    }
                }
            }
            else
            {
                _lastHitPoint = null;
            }

            currentView.Repaint();

            if (currentEvent.type == EventType.Repaint && _lastHitPoint.HasValue)
            {
                if (!CanPaint())
                    Handles.color = _data.disabledCursorColor;
                else
                    Handles.color = _data.defaultCursorColor;

                Handles.DrawWireDisc(_lastHitPoint.Value, _hit.normal, _data.brushSize, _data.cursorThickness);
                
                Handles.color = _handlesDefaultColor;
            }

        }

        #region // OnSceneGUI

        private bool CanPaint()
        {
            if (_data.parentContainer == null)
                return false;

            if (_data.objects.Count <= 0)
            {
                return false;
            }

            if (!_lastHitPoint.HasValue)
                return false;

            return true;
        }

        private void Paint()
        {
            if (!CanPaint())
                return;

            int id;
            if (_data.randomObject)
                id = Random.Range(0, _objects.arraySize);
            else
                id = _selectedIndex;

            switch (_data.brushType)
            {
                case InstantiateBrushData.BrushType.Eraser:
                    EraserBrush();
                    break;
                case InstantiateBrushData.BrushType.Stamp:
                    StampBrush(id);
                    break;
                /*case InstantiateBrushData.BrushType.Spread:
                    SpreadBrush(id);
                    break;*/
                case InstantiateBrushData.BrushType.Spray:
                    SprayBrush(id);
                    break;
            }
        }

        private void EraserBrush()
        {
            for (int i = 0; i < _data.parentContainer.childCount; i++)
            {
                if (Vector3.Distance(_data.parentContainer.GetChild(i).position, _hit.point) <= _data.brushSize)
                {
                    GameObject obj = _data.parentContainer.GetChild(i).gameObject;
                    Undo.DestroyObjectImmediate(obj);
                }
            }

            _lastPaintedPoint = _hit.point;
        }

        private void StampBrush(int id)
        {
            UndoRegisterCreate(InstantiateObject(id, _hit));
        }

        /*private void SpreadBrush(int id)
        {
            if (_lastPaintedPoint.HasValue)
            {
                if (_data.instanceDistance > Vector3.Distance(_lastPaintedPoint.Value, _lastHitPoint.Value))
                    return;
            }

            UndoRegisterCreate(InstantiateObject(id, _hit));
        }*/

        private void SprayBrush(int id)
        {           
            if (_lastPaintedPoint.HasValue)
            {
                if (_data.instanceDistance > Vector3.Distance(_lastPaintedPoint.Value, _lastHitPoint.Value))
                    return;
            }

            for (int i = 0; i < _data.density; i++)
            {
                Vector2 randonPoint = Random.insideUnitCircle * _data.brushSize;
                Vector3 pos = _hit.point + new Vector3(randonPoint.x, 0f, randonPoint.y);
                if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, 10f))
                {
                    UndoRegisterCreate(InstantiateObject(id, hit));
                }
            }
        }

        private GameObject InstantiateObject(int id, RaycastHit hit)
        {
            GameObject obj;
            if (_instantiateModesIndex == 0)
            {
                obj = PrefabUtility.InstantiatePrefab(_data.objects[id]) as GameObject;
            }
            else
            {
                obj = Instantiate(_data.objects[id]);
            }
            
            obj.transform.position = hit.point;
            obj.transform.up = hit.normal;
            obj.transform.parent = _data.parentContainer;
            _lastPaintedPoint = hit.point;

            if (_data.randomRotationY)
            {
                float randY = Random.Range(0f, 360f);
                obj.transform.localRotation = Quaternion.Euler(obj.transform.localEulerAngles.x, randY, obj.transform.localEulerAngles.z);
            }

            obj.transform.localScale *= Random.Range(_data.randomScale, 1f);

            return obj;
        }

        private void UndoRegisterCreate(Object obj)
        {
            Undo.RegisterCreatedObjectUndo(obj, $"InstantiateBrush - Create: {obj.name}");
        }

        #endregion
    }
#endif

    public class InstantiateBrushData : ScriptableObject
    {
        public enum BrushType
        {
            Eraser,
            Stamp,
            Spray,
        }

        #region // Settings

        [Tooltip("Cursor color when can't use the 'Brush'")]
        [SerializeField] public Color disabledCursorColor = Color.red;
        [Tooltip("Cursor color when can use the 'Brush'")]
        [SerializeField] public Color defaultCursorColor = Color.white;
        [Tooltip("Thickness of the 'Brush' cursor")]
        [SerializeField, Min(1f)] public float cursorThickness = 1f;

        #endregion

        #region // Parameters

        // Header
        [Tooltip("Object that will contain the instantiated objects. The 'Eraser' only erases child objects of 'Parent Container', whether it was instantiated by the 'Brush' or not.")]
        [SerializeField] public Transform parentContainer;
        [Tooltip("Layers that the 'Brush' will affect (the target object must have a 'Collider' for the layer to be identified).")]
        [SerializeField] public LayerMask targetLayers;
        [Tooltip("'Brush' behaviour. Eraser: erases objects. Stamp: instantiates an object on click, designed for precision. Spray: instantiated while moving the cursor, designed for random and natural placement.")]
        [SerializeField] public BrushType brushType = BrushType.Stamp;

        // Brush size
        [Tooltip("'Brush' effect area size.")]
        [SerializeField, Range(0.1f, 20f)] public float brushSize = 1f;

        // Density
        [Tooltip("Number of objects instantiated at once.")]
        [SerializeField, Min(1)] public int density = 1;

        // Spread
        [Tooltip("Minimum spacing distance between one instantiation and another.")]
        [SerializeField] public float instanceDistance = 2f;

        // Object variables
        [Tooltip("Randomizes the instantiated object among the objects in the list.")]
        [SerializeField] public bool randomObject;
        [Tooltip("Randomizes the Y axis rotation of the instantiated object.")]
        [SerializeField] public bool randomRotationY = false;
        [Tooltip("Randomizes a instantiated object scale multiplier between the defined value and 1. If the value is equals to 1, the randomization is disabled")]
        [SerializeField, Range(0f, 1f)] public float randomScale = 0.5f;

        // Footer    
        [Tooltip("List of objects that maust be instantiateds by the 'Brush'; like a palette.")]
        [SerializeField] public List<GameObject> objects = new List<GameObject>();

        #endregion

        
        
        /*[ContextMenu("ClearList")]
        public void ClearList()
        {
            objects.Clear();
        }*/
    }
}