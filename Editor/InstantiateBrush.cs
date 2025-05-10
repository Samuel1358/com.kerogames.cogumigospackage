using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InstantiateBrush : MonoBehaviour
{
    public enum BrushType
    {
        Stamp,
        Spread,
        //Spray
    }

    [SerializeField] public Transform targetParent;

    [SerializeField] public BrushType brushType = BrushType.Spread;

    [SerializeField] public float instanceDistance = 2f;
    [SerializeField] public bool randomObject;
    [SerializeField] public bool randomRotationY = false;
    [SerializeField]
    [Range(0f, 1f)] public float randomScale = 0.5f;

    [SerializeField] public LayerMask includeLayers;
    [SerializeField] public List<GameObject> objects = new List<GameObject>();
    private int _selectedIndex = 0;
    [SerializeField] public int selectedIndex { get { if (_selectedIndex > objects.Count) _selectedIndex = 0; return _selectedIndex; } set { if (value < objects.Count) _selectedIndex = value; else _selectedIndex = 0; } }

    // Get & Set
    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    [ContextMenu("ClearList")]
    public void ClearList()
    {
        objects.Clear();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InstantiateBrush))]
public class InstantiateBrushInspector : Editor
{
    private SerializedProperty _script;

    private InstantiateBrush _instantiateBrush;
    private SerializedObject _serializedObject;

    private SerializedProperty _targetParent;

    private SerializedProperty _brushType;

    private SerializedProperty _instanceDistance;
    private SerializedProperty _randomObject;
    private SerializedProperty _randomRotationY;
    private SerializedProperty _randomScale;

    private SerializedProperty _includeLayers;
    private SerializedProperty _objects;

    private RaycastHit _hit;
    private Vector3? _lastHitPoint;
    private bool _isPainting;
    private Vector3? _lastPaintedPoint;

    private void OnEnable()
    {
        _script = serializedObject.FindProperty("m_Script");

        _instantiateBrush = target as InstantiateBrush;
        _serializedObject = new SerializedObject(_instantiateBrush);

        _targetParent = _serializedObject.FindProperty("targetParent");

        _brushType = _serializedObject.FindProperty("brushType");

        _instanceDistance = _serializedObject.FindProperty("instanceDistance");
        _randomObject = _serializedObject.FindProperty("randomObject");
        _randomRotationY = _serializedObject.FindProperty("randomRotationY");
        _randomScale = _serializedObject.FindProperty("randomScale");

        _includeLayers = _serializedObject.FindProperty("includeLayers");
        _objects = _serializedObject.FindProperty("objects");
    }

    public override void OnInspectorGUI()
    {
        ScriptReferance();

        SerializedFields();

        int id = _instantiateBrush.selectedIndex;
        GUICustomElements.FlexibleSelectionGrid(ref id, _instantiateBrush.objects);
        _instantiateBrush.selectedIndex = id;
    }

    #region // OnInspectorGUI

    private void ScriptReferance()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_script, true);
        EditorGUI.EndDisabledGroup();
    }

    private void SerializedFields()
    {
        _serializedObject.Update();

        EditorGUILayout.PropertyField(_targetParent);

        if (_instantiateBrush.targetParent == null)
            EditorGUILayout.HelpBox("Without a TargetParent reference the object gonna be instantiated directly in the scene!", MessageType.Warning);

        EditorGUILayout.PropertyField(_brushType);
        _instantiateBrush.brushType = (InstantiateBrush.BrushType)_brushType.enumValueIndex;

        if (_instantiateBrush.brushType == InstantiateBrush.BrushType.Spread)
            EditorGUILayout.PropertyField(_instanceDistance);
        else
            GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(_randomObject);
        EditorGUILayout.PropertyField(_randomRotationY);

        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(_randomScale);

        EditorGUILayout.PropertyField(_includeLayers);

        EditorGUILayout.PropertyField(_objects);

        _serializedObject.ApplyModifiedProperties();
    }

    #endregion

    private void OnSceneGUI()
    {
        Event currentEvent = Event.current;

        SceneView currentView = SceneView.currentDrawingSceneView;
        if (currentView == null || currentView.camera == null)
            return;

        Vector2 mousePosition = currentEvent.mousePosition;
        mousePosition.y = currentView.camera.pixelHeight - mousePosition.y;
        Ray ray = currentView.camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out _hit, Mathf.Infinity, _instantiateBrush.includeLayers))
        {
            //Debug.Log("Mouse está sobre: " + _hit.collider.gameObject.name);
            _lastHitPoint = _hit.point;

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                _isPainting = true;
                Paint();
                currentEvent.Use();
            }

            if (_instantiateBrush.brushType == InstantiateBrush.BrushType.Spread)
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
            Handles.DrawWireDisc(_lastHitPoint.Value, _hit.normal, 1f);
    }

    #region // OnSceneGUI

    private void Paint()
    {
        if (_instantiateBrush.objects.Count <= 0)
        {
            Debug.LogWarning("InstantiateBrush warning: No object assigned!");
            return;
        }

        if (!_lastHitPoint.HasValue)
            return;

        int id;
        if (_instantiateBrush.randomObject)
            id = Random.Range(0, _objects.arraySize);
        else
            id = _instantiateBrush.selectedIndex;

        switch (_instantiateBrush.brushType)
        {
            case InstantiateBrush.BrushType.Stamp:
                StampBrush(id);
                break;
            case InstantiateBrush.BrushType.Spread:
                SpreadBrush(id);
                break;
        }       
    }

    private void StampBrush(int id)
    {
        GameObject obj = Instantiate(_instantiateBrush.objects[id], _hit.point, Quaternion.identity, _instantiateBrush.targetParent);
        _lastPaintedPoint = _hit.point;

        if (_instantiateBrush.randomRotationY)
        {
            float randY = Random.Range(0f, 360f);
            obj.transform.localRotation = Quaternion.Euler(obj.transform.localEulerAngles.x, randY, obj.transform.localEulerAngles.z);
        }

        obj.transform.localScale *= Random.Range(_instantiateBrush.randomScale, 1f);

        UndoRegister(obj);
    }

    private void SpreadBrush(int id)
    {
        if (_lastPaintedPoint.HasValue)
        {
            if (_instantiateBrush.instanceDistance > Vector3.Distance(_lastPaintedPoint.Value, _lastHitPoint.Value))
                return;
        }

        GameObject obj = Instantiate(_instantiateBrush.objects[id], _hit.point, Quaternion.identity, _instantiateBrush.targetParent);
        _lastPaintedPoint = _hit.point;

        if (_instantiateBrush.randomRotationY)
        {
            float randY = Random.Range(0f, 360f);
            obj.transform.localRotation = Quaternion.Euler(obj.transform.localEulerAngles.x, randY, obj.transform.localEulerAngles.z);
        }       

        obj.transform.localScale *= Random.Range(_instantiateBrush.randomScale, 1f);

        UndoRegister(obj);
    }

    private void UndoRegister(Object obj)
    {
        Undo.RegisterCreatedObjectUndo(obj, $"InstantiateBrush: {obj.name}");
    }

    #endregion
}
#endif
