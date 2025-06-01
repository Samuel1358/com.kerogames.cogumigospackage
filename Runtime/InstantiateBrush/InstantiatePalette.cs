using System.Collections.Generic;
using UnityEngine;

namespace CogumigosPackage.InstantiateBrush
{
    [CreateAssetMenu(fileName = "InstantiatePalette", menuName = "Instantiate Brush/InstantiatePalette")]
    public class InstantiatePalette : ScriptableObject
    {
        [SerializeField] private List<GameObject> _objects;

        public List<GameObject> Objects { get { return _objects; } }
    }
}
