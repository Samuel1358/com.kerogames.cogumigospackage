using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace CogumigosPackage.Editor.Window
{
#if UNITY_EDITOR
    public class InstantiateBrushWindow : EditorWindow
    {
        private InstantiateBrushData _data;
        private SerializedObject _serializedObject;

        // header obrigatório
        private SerializedProperty _parentContainer;
        private SerializedProperty _targetLayers;
        private SerializedProperty _brushType;

        // sizable brush
        private SerializedProperty _brushSize;

        // densidade
        private SerializedProperty _density;

        // spread
        private SerializedProperty _instanceDistance;

        // instantiate obj
        private SerializedProperty _randomObject;
        private SerializedProperty _randomRotationY;
        private SerializedProperty _randomScale;

        private string[] _instantiateModes = { "Prefab", "Clone" };
        private int _instantiateModesIndex = 0;

        // footer obrigatório       
        private SerializedProperty _objects;

        private RaycastHit _hit;
        private Vector3? _lastHitPoint;
        private bool _isPainting;
        private Vector3? _lastPaintedPoint;

        private Color _handlesDefaultColor;

        private void OnEnable()
        {
            _data = CreateInstance(typeof(InstantiateBrushData)) as InstantiateBrushData;
            _serializedObject = new SerializedObject(_data);

            // header obrigatório
            _parentContainer = _serializedObject.FindProperty("parentContainer");
            _targetLayers = _serializedObject.FindProperty("targetLayers");
            _brushType = _serializedObject.FindProperty("brushType");

            // sizable brush
            _brushSize = _serializedObject.FindProperty("brushSize");

            // densidade
            _density = _serializedObject.FindProperty("density");

            // spread
            _instanceDistance = _serializedObject.FindProperty("instanceDistance");

            // instantiate obj
            _randomObject = _serializedObject.FindProperty("randomObject");
            _randomRotationY = _serializedObject.FindProperty("randomRotationY");
            _randomScale = _serializedObject.FindProperty("randomScale");

            // footer obrigatório           
            _objects = _serializedObject.FindProperty("objects");

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
            SerializedFields();

            int id = _data.selectedIndex;
            GUICustomElements.FlexibleSelectionGrid(ref id, _data.objects);
            _data.selectedIndex = id;
        }

        #region // OnInspectorGUI

        private void SerializedFields()
        {
            _serializedObject.Update();

            // header obrigatório
            EditorGUILayout.PropertyField(_parentContainer);

            if (_data.parentContainer == null)
                EditorGUILayout.HelpBox("'Parent Container' need to be assigned!", MessageType.Error);

            EditorGUILayout.PropertyField(_targetLayers);
            EditorGUILayout.PropertyField(_brushType);
            _data.brushType = (InstantiateBrushData.BrushType)_brushType.enumValueIndex;

            #region // sizable brush

            if (_data.brushType == InstantiateBrushData.BrushType.Eraser || _data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_brushSize);
            }

            #endregion

            #region // densidade

            if (_data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_density);
            }

            #endregion

            #region // spread

            if (_data.brushType == InstantiateBrushData.BrushType.Spread || _data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_instanceDistance);
            }

            #endregion

            #region // instantiate obj

            if (_data.brushType == InstantiateBrushData.BrushType.Stamp || _data.brushType == InstantiateBrushData.BrushType.Spread || _data.brushType == InstantiateBrushData.BrushType.Spray)
            {
                EditorGUILayout.PropertyField(_randomObject);
                EditorGUILayout.PropertyField(_randomRotationY);

                EditorGUILayout.PropertyField(_randomScale);

                if ((_data.objects.Count > 0) ? PrefabUtility.IsPartOfPrefabAsset(_data.objects[_data.selectedIndex]) : false)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Instantiate mode:");
                    GUICustomElements.FlexibleSelectionGrid(ref _instantiateModesIndex, _instantiateModes);

                    GUILayout.EndHorizontal();
                }
                else
                    _instantiateModesIndex = 1;
                
            }           

            #endregion

            // footer obrigatório
            EditorGUILayout.PropertyField(_objects);

            if (_data.objects.Count <= 0 && _data.brushType == InstantiateBrushData.BrushType.Eraser)
                EditorGUILayout.HelpBox("The eraser don't work while 'Objects' list is empty!", MessageType.Warning);

            _serializedObject.ApplyModifiedProperties();
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

                if (_data.brushType == InstantiateBrushData.BrushType.Eraser || _data.brushType == InstantiateBrushData.BrushType.Spread || _data.brushType == InstantiateBrushData.BrushType.Spray)
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
                    Handles.color = Color.red;
                else
                    Handles.color = _handlesDefaultColor;

                Handles.DrawWireDisc(_lastHitPoint.Value, _hit.normal, _data.brushSize);
                
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
                id = _data.selectedIndex;

            switch (_data.brushType)
            {
                case InstantiateBrushData.BrushType.Eraser:
                    EraserBrush();
                    break;
                case InstantiateBrushData.BrushType.Stamp:
                    StampBrush(id);
                    break;
                case InstantiateBrushData.BrushType.Spread:
                    SpreadBrush(id);
                    break;
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

        private void SpreadBrush(int id)
        {
            if (_lastPaintedPoint.HasValue)
            {
                if (_data.instanceDistance > Vector3.Distance(_lastPaintedPoint.Value, _lastHitPoint.Value))
                    return;
            }

            UndoRegisterCreate(InstantiateObject(id, _hit));
        }

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
                if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
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
            Spread,
            Spray
        }

        // header obrigatorio
        [SerializeField] public Transform parentContainer;
        [SerializeField] public LayerMask targetLayers;
        [SerializeField] public BrushType brushType = BrushType.Spread;

        // sizable brush
        [SerializeField, Range(0.1f, 20f)] public float brushSize = 1f;

        // densidade
        [SerializeField, Min(1)] public int density = 1;

        // spread
        [SerializeField] public float instanceDistance = 2f;

        // instantiate obj
        [SerializeField] public bool randomObject;
        [SerializeField] public bool randomRotationY = false;
        [SerializeField, Range(0f, 1f)] public float randomScale = 0.5f;

        // footer obrigatorio       
        [SerializeField] public List<GameObject> objects = new List<GameObject>();

        public int selectedIndex = 0;
        
        [ContextMenu("ClearList")]
        public void ClearList()
        {
            objects.Clear();
        }
    }
}