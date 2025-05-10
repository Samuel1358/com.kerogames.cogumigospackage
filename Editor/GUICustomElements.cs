using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CogumigosPackage.Editor
{
#if UNITY_EDITOR
    public static class GUICustomElements
    {
        private static Color lightBlue = new Color(0.5490196078431373f, 0.7450980392156863f, 0.9803921568627451f);

        #region // Dialog



        #endregion

        #region // ObjectRegister

        public static void ObjectRegister<T>(ref T obj, List<T> list, bool listRevise = true) where T : Object
        {
            m_ObjectRegister(ref obj, list);

            if (listRevise)
            {
                m_ListRevise(list);
            }

            if (obj == null)
            {
                EditorGUILayout.HelpBox("Can't be register without a reference", MessageType.None);
            }
        }

        public static void ListObjectRegister<T>(ref int submitCount, ref bool isListOpened, ref List<T> submitList, List<T> registerList, bool listRevise = true) where T : Object
        {
            m_ListObjectRegister(ref submitCount, ref isListOpened, ref submitList, registerList);

            if (listRevise)
            {
                m_ListRevise(registerList);
            }
        }

        private static void m_ObjectRegister<T>(ref T obj, List<T> list) where T : Object
        {
            GUILayout.BeginHorizontal();

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
            obj = (T)EditorGUILayout.ObjectField(obj, typeof(T));

            bool contains = list.Contains(obj as T);

            if (GUILayout.Button((contains) ? "Remove" : "Register"))
            {
                if (obj != null)
                {
                    if (contains)
                    {
                        list.Remove(obj as T);
                        obj = null;
                    }
                    else
                    {
                        list.Add(obj as T);
                        obj = null;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void m_ListObjectRegister<T>(ref int submitCount, ref bool isListOpened, ref List<T> submitList, List<T> registerList) where T : Object
        {
            GUILayout.BeginHorizontal();

            isListOpened = EditorGUILayout.BeginFoldoutHeaderGroup(isListOpened, "Submit List");

            submitCount = EditorGUILayout.IntField((submitCount < 1) ? 1 : submitCount, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f), GUILayout.MinWidth(54), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button("Submit"))
            {
                m_RegisterSubmitList(ref submitList, registerList);
            }

            GUILayout.EndHorizontal();

            if (isListOpened)
            {
                if (submitList.Count < submitCount)
                {
                    for (int i = submitList.Count; i <= submitCount; i++)
                    {
                        submitList.Add(null);
                    }
                }
                for (int i = 0; i < submitCount; i++)
                {
#pragma warning disable CS0618 // O tipo ou membro é obsoleto
                    submitList[i] = (T)EditorGUILayout.ObjectField(submitList[i], typeof(T));
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void m_RegisterSubmitList<T>(ref List<T> submitList, List<T> registerList) where T : Object
        {
            List<T> list = new List<T>();
            foreach (T t in submitList)
            {
                list.Add(t);
            }

            T[] aux = list.ToArray();
            for (int i = 0; i < aux.Length; i++)
            {
                if (aux[i] != null)
                {
                    if (registerList.Contains(aux[i] as T))
                    {
                        registerList.Remove(aux[i] as T);
                        aux[i] = null;
                    }
                    else
                    {
                        registerList.Add(aux[i] as T);
                        aux[i] = null;
                    }
                }
            }

            submitList.Clear();
        }

        private static void m_ListRevise<T>(List<T> list)
        {
            List<T> aux = new List<T>();
            foreach (T t in list)
            {
                if (t != null)
                {
                    aux.Add(t);
                }
            }

            list = aux;
        }

        #endregion

        #region // FlexibleSelectionGrid

        #region // Regular

        public static void FlexibleSelectionGrid(ref int selectedIndex, string[] namesList)
        {
            bool changed;
            m_FlexibleSelectionGrid(ref selectedIndex, namesList, out changed);
        }
        public static void FlexibleSelectionGrid(ref int selectedIndex, string[] namesList, out bool changed)
        {
            m_FlexibleSelectionGrid(ref selectedIndex, namesList, out changed);
        }

        public static void FlexibleSelectionGrid<T>(ref int selectedIndex, List<T> list)
        {
            bool changed;
            m_FlexibleSelectionGrid(ref selectedIndex, m_GetSelectionNames(list), out changed);
        }
        public static void FlexibleSelectionGrid<T>(ref int selectedIndex, List<T> list, out bool changed)
        {
            m_FlexibleSelectionGrid(ref selectedIndex, m_GetSelectionNames(list), out changed);
        }

        #endregion

        #region // Format

        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, int alignFactor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, namesList, alignFactor, lightBlue);
        }
        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, int alignFactor, Color selectionColor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, namesList, alignFactor, selectionColor);
        }
        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, Color selectionColor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, namesList, 8, selectionColor);
        }

        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, out bool changed, int alignFactor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, namesList, out changed, alignFactor, lightBlue);
        }
        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, out bool changed, int alignFactor, Color selectionColor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, namesList, out changed, alignFactor, selectionColor);
        }
        public static void FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, out bool changed, Color selectionColor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, namesList, out changed, 8, selectionColor);
        }

        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, int alignFactor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, m_GetSelectionNames(list), alignFactor, lightBlue);
        }
        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, int alignFactor, Color selectionColor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, m_GetSelectionNames(list), alignFactor, selectionColor);
        }
        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, Color selectionColor)
        {
            m_FlexibleSelectionGridJustFormat(ref selectedIndex, m_GetSelectionNames(list), 8, selectionColor);
        }

        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, out bool changed, int alignFactor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, m_GetSelectionNames(list), out changed, alignFactor, lightBlue);
        }
        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, out bool changed, int alignFactor, Color selectionColor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, m_GetSelectionNames(list), out changed, alignFactor, selectionColor);
        }
        public static void FlexibleSelectionGridFormat<T>(ref int selectedIndex, List<T> list, out bool changed, Color selectionColor)
        {
            m_FlexibleSelectionGridFormat(ref selectedIndex, m_GetSelectionNames(list), out changed, 8, selectionColor);
        }

        #endregion


        private static void m_FlexibleSelectionGrid(ref int selectedIndex, string[] namesList, out bool changed)
        {
            GUILayout.BeginHorizontal();

            changed = false;

            int wdith = 0;
            for (int i = 0; i < namesList.Length; i++)
            {
                wdith += namesList[i].Length * 8;
                if (wdith < EditorGUIUtility.currentViewWidth)
                {
                    Color oldColor = GUI.backgroundColor;

                    if (selectedIndex == i)
                    {
                        GUI.backgroundColor = lightBlue;
                    }

                    // return index
                    if (GUILayout.Button(namesList[i]))
                    {
                        selectedIndex = i;
                        changed = true;
                    }

                    GUI.backgroundColor = oldColor;
                }
                else
                {
                    wdith = 0;
                    i--;

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void m_FlexibleSelectionGridFormat(ref int selectedIndex, string[] namesList, out bool changed, int alignFactor, Color selectionColor)
        {
            GUILayout.BeginHorizontal();

            changed = false;

            int wdith = 0;
            for (int i = 0; i < namesList.Length; i++)
            {
                wdith += namesList[i].Length * alignFactor;
                if (wdith < EditorGUIUtility.currentViewWidth)
                {
                    Color oldColor = GUI.backgroundColor;

                    if (selectedIndex == i)
                    {
                        GUI.backgroundColor = selectionColor;
                    }

                    // return index
                    if (GUILayout.Button(namesList[i]))
                    {
                        selectedIndex = i;
                        changed = true;
                    }

                    GUI.backgroundColor = oldColor;
                }
                else
                {
                    wdith = 0;
                    i--;

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void m_FlexibleSelectionGridJustFormat(ref int selectedIndex, string[] namesList, int alignFactor, Color selectionColor)
        {
            GUILayout.BeginHorizontal();

            int wdith = 0;
            for (int i = 0; i < namesList.Length; i++)
            {
                wdith += namesList[i].Length * alignFactor;
                if (wdith < EditorGUIUtility.currentViewWidth)
                {
                    Color oldColor = GUI.backgroundColor;

                    if (selectedIndex == i)
                    {
                        GUI.backgroundColor = selectionColor;
                    }

                    // return index
                    if (GUILayout.Button(namesList[i]))
                    {
                        selectedIndex = i;
                    }

                    GUI.backgroundColor = oldColor;
                }
                else
                {
                    wdith = 0;
                    i--;

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();
        }

        private static string[] m_GetSelectionNames<T>(List<T> list)
        {
            List<string> names = new List<string>();
            foreach (T item in list)
            {
                if (item is Object o)
                {
                    if (o != null)
                    {
                        names.Add(o.name);
                    }
                }
                else
                {
                    if (item != null)
                    {
                        names.Add(item.ToString());
                    }
                }
            }

            return names.ToArray();
        }

        #endregion
    }
#endif
}
