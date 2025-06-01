using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CogumigosPackage.Multiprefab
{
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
}
