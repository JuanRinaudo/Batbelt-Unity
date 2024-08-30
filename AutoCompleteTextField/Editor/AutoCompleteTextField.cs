#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EditorGUIExtras = UnityEditor.AutoCompleteTextField.EditorGUI;

namespace UnityEditor
{
    public class AutoCompleteTextField : EditorWindow
    {
        public static bool changedLastFrame = false;

        public static class EditorGUILayout
        {
            public static string AutoCompleteTextField(string text, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField("", text, GUI.skin.textField, entries, "", options);
            }

            public static string AutoCompleteTextField(string text, string[] entries, string hint, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField("", text, GUI.skin.textField, entries, hint, options);
            }

            public static string AutoCompleteTextField(string text, GUIStyle style, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField("", text, style, entries, "", options);
            }

            public static string AutoCompleteTextField(string text, GUIStyle style, string[] entries, string hint, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField("", text, style, entries, hint, options);
            }

            public static string AutoCompleteTextField(string label, string text, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, GUI.skin.textField, entries, "", options);
            }

            public static string AutoCompleteTextField(string label, string text, string[] entries, string hint, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, GUI.skin.textField, entries, hint, options);
            }

            public static string AutoCompleteTextField(string label, string text, GUIStyle style, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, style, entries, "", options);
            }

            public static string AutoCompleteTextField(string label, string text, GUIStyle style, string[] entries, string hint, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(new GUIContent(label), text, style, entries, hint, options);
            }

            public static string AutoCompleteTextField(GUIContent label, string text, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, GUI.skin.textField, entries, "", options);
            }

            public static string AutoCompleteTextField(GUIContent label, string text, string[] entries, string hint, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, GUI.skin.textField, entries, hint, options);
            }

            public static string AutoCompleteTextField(GUIContent label, string text, GUIStyle style, string[] entries, params GUILayoutOption[] options)
            {
                return AutoCompleteTextField(label, text, style, entries, "", options);
            }

            public static string AutoCompleteTextField(GUIContent label, string text, GUIStyle style, string[] entries, string hint, params GUILayoutOption[] options)
            {
                //Get the rect to draw the text field
                Rect lastRect = UnityEditor.EditorGUILayout.GetControlRect(!string.IsNullOrEmpty(label.text), EditorGUIUtility.singleLineHeight, style, options);

                //Draw it without using layout
                if(changedLastFrame)
                {
                    GUI.changed = true;
                    changedLastFrame = false;
                }
                return EditorGUIExtras.AutoCompleteTextField(lastRect, label, text, style, entries, hint);;
            }
        }

        public static class EditorGUI
        {
            public static string AutoCompleteTextField(Rect position, string text, string[] entries)
            {
                return AutoCompleteTextField(position, "", text, GUI.skin.textField, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, string text, string[] entries, string hint)
            {
                return AutoCompleteTextField(position, "", text, GUI.skin.textField, entries, hint);
            }

            public static string AutoCompleteTextField(Rect position, string text, GUIStyle style, string[] entries)
            {
                return AutoCompleteTextField(position, "", text, style, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, string text, GUIStyle style, string[] entries, string hint)
            {
                return AutoCompleteTextField(position, "", text, style, entries, hint);
            }

            public static string AutoCompleteTextField(Rect position, string label, string text, string[] entries)
            {
                return AutoCompleteTextField(position, label, text, GUI.skin.textField, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, string label, string text, string[] entries, string hint)
            {
                return AutoCompleteTextField(position, label, text, GUI.skin.textField, entries, hint);
            }

            public static string AutoCompleteTextField(Rect position, string label, string text, GUIStyle style, string[] entries)
            {
                return AutoCompleteTextField(position, label, text, style, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, string label, string text, GUIStyle style, string[] entries, string hint)
            {
                return AutoCompleteTextField(position, new GUIContent(label), text, style, entries, hint);
            }

            public static string AutoCompleteTextField(Rect position, GUIContent label, string text, string[] entries)
            {
                return AutoCompleteTextField(position, label, text, GUI.skin.textField, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, GUIContent label, string text, string[] entries, string hint)
            {
                return AutoCompleteTextField(position, label, text, GUI.skin.textField, entries, hint);
            }

            public static string AutoCompleteTextField(Rect position, GUIContent label, string text, GUIStyle style, string[] entries)
            {
                return AutoCompleteTextField(position, label, text, style, entries, "");
            }

            public static string AutoCompleteTextField(Rect position, GUIContent label, string text, GUIStyle style, string[] entries, string hint)
            {
                //Check for focus, the system is only shown if the text field is focused
                GUI.SetNextControlName("CheckFocus");
                string value = UnityEditor.EditorGUI.TextField(position, label, text, style);

                return _AutoCompleteLogic(position, label, value, entries, hint);
            }
        }

        public static float WindowMaxHeight = 200;

        //Used to ensure only 1 auto complete window is displayed
        static AutoCompleteTextField reference;

        //This is a #Hack to prevent saving the return value to the wrong UI element
        static Vector2 oldPosition;
        //Saves the returned value (if any was selected)
        static GUIContent returnedContent = new GUIContent();
        static GUIStyle bigTitle;
        static GUIStyle selectedStyle;
        static Color selectedColor = Color.black;

        static bool returnedValue;

        //If the list of elements is big enough this saves the scroll position
        Vector2 scrollPosition = Vector2.zero;
        //Rect for the scroll view content
        Rect scrollViewContent = new Rect(0, 0, 0, 0);
        //Rect for the window
        Rect autoCompleteRect;
        //Entried for the auto complete window
        string[] entries;
        //Input to show on text field
        string input;
        //Selection index
        int selectedIndex = -1;

        private static string _AutoCompleteLogic(Rect position, GUIContent label, string text, string[] entries, string hint)
        {
            if (bigTitle == null)
            {
                bigTitle = new GUIStyle("In BigTitle");
            }

            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle
                {
                    normal = new GUIStyleState { background = Texture2D.whiteTexture }
                };
            }

            if (selectedColor == Color.black)
            {
                selectedColor = GUI.skin.settings.selectionColor;
                selectedColor.a = 0.45f;
            }

            //Used to draw the window
            Rect lastRect = position;

            if (returnedValue)
            {
                float offset = 0;

                if (!string.IsNullOrEmpty(label.text))
                {
                    offset = UnityEditor.EditorGUIUtility.labelWidth - ((UnityEditor.EditorGUI.indentLevel) * 15);
                }

                Vector2 pos = GUIUtility.GUIToScreenPoint(new Vector2(lastRect.x, lastRect.y));

                pos.x += UnityEditor.EditorGUI.indentLevel * 15 + offset;

                //The system returned a value, need to check if this UI element is the one that called
                if (pos.Equals(oldPosition))
                {
                    returnedValue = false;
                    string val = returnedContent.text;
                    returnedContent.text = "";
                    return val;
                }
            }

            //Hack to avoid hint being drawn on the wrong location for attribute drawers
            GUIStyle myStyle = new GUIStyle(GUIStyle.none);

            myStyle.normal.textColor = Color.white * 0;
            GUI.enabled = false;
            UnityEditor.EditorGUI.TextField(lastRect, " ", " ", myStyle);
            GUI.enabled = true;

            //Display the hint
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(hint))
            {
                //Nothing is typed, show a hint to the user
                GUI.enabled = false;

                UnityEditor.EditorGUI.TextField(lastRect, label, " " + hint, GUI.skin.label);
                GUI.enabled = true;
            }

            //Only display the system if the text field is focused
            if (GUI.GetNameOfFocusedControl().Equals("CheckFocus"))
            {
                //We should only call the window the event is on repaint
                if (Event.current.type == EventType.Repaint)
                {
                    float currentSize = (entries.Length + 1) * 22;
                    lastRect.height = currentSize > WindowMaxHeight ? WindowMaxHeight : currentSize;

                    Vector2 pos = GUIUtility.GUIToScreenPoint(new Vector2(lastRect.x, lastRect.y));

                    //New position for this window
                    Rect newRect = new Rect(pos.x, pos.y, lastRect.width, lastRect.height);

                    //Remove focus
                    GUI.FocusControl("");

                    if (!string.IsNullOrEmpty(label.text))
                    {
                        float offset = UnityEditor.EditorGUIUtility.labelWidth - ((UnityEditor.EditorGUI.indentLevel) * 15);
                        newRect.x += offset;
                        newRect.width -= offset;
                    }

                    newRect.x += UnityEditor.EditorGUI.indentLevel * 15;
                    newRect.width -= UnityEditor.EditorGUI.indentLevel * 15;

                    CheckReference(newRect);
                    reference.input = text;
                    reference.entries = entries;
                }
            }

            return text;
        }

        private static void CheckReference(Rect textFieldRect)
        {
            if (reference == null)
            {
                reference = CreateInstance<AutoCompleteTextField>();
                reference.ShowAsDropDown(textFieldRect, new Vector2(textFieldRect.width, textFieldRect.height));
                reference.position = textFieldRect;
                oldPosition = textFieldRect.position;
            }
        }

        private void OnGUI()
        {
            //If the input is null the TextField will throw an error
            if (string.IsNullOrEmpty(input))
                input = "";

            //Focus this new textfield
            GUI.SetNextControlName("TextField");

            UnityEditor.EditorGUI.BeginChangeCheck();
            string newInput = GUILayout.TextField(input);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                if (!newInput.Equals(input))
                    selectedIndex = -1;

                input = newInput;
            }
            GUI.FocusControl("TextField");

            autoCompleteRect = GUILayoutUtility.GetLastRect();

            DoWindowUpdated();
        }

        void AutoScroll()
        {
            float desiredScroll = (float)selectedIndex * 22f;

            float fitHeight = position.height - 40f + scrollPosition.y;

            if (scrollPosition.y > desiredScroll) //Scroll 
            {
                scrollPosition.y = desiredScroll;
            }
            else if (desiredScroll > fitHeight) //Scroll down
            {
                scrollPosition.y += desiredScroll - fitHeight;
            }

            Repaint();
        }

        private void DoWindowUpdated()
        {
            //Adjust the rect for the auto complete group
            autoCompleteRect.y += autoCompleteRect.height + 2;

            //array of the elements that match the already typed data
            List<string> matchElements = new List<string>();

            //If nothing is typed, show the entire list
            if (string.IsNullOrEmpty(input))
            {
                matchElements = entries.ToList();
            }
            else
            {
                List<string> entriesTemp = new List<string>();
                matchElements = entries.Where(data => data.ToLower().Contains(input.ToLower())).ToList();

                //Filter the list with the typed data
                foreach (var entry in matchElements)
                {
                    string entryTemp = entry;

                    if (!entryTemp.ToLower().Contains(input.ToLower()))
                        continue;

                    entriesTemp.Add(entryTemp);
                }

                matchElements = entriesTemp;
            }

            //Calculate the correct size of the window based on how many lements are visible
            float currentSize = matchElements.Count * 22;
            minSize = Vector2.zero;

            if (currentSize < WindowMaxHeight)
            {
                autoCompleteRect.height = currentSize;
                maxSize = new Vector2(maxSize.x, currentSize + 22);
            }
            else
            {
                maxSize = new Vector2(maxSize.x, WindowMaxHeight);
                autoCompleteRect.height = maxSize.y - (autoCompleteRect.height + 4);
            }

            position = new Rect(oldPosition.x, oldPosition.y, maxSize.x, maxSize.y);

            scrollViewContent.width = autoCompleteRect.width - 20;

            GUI.BeginGroup(autoCompleteRect);

            if (Event.current.type == EventType.Repaint)
            {
                //Draw background
                bigTitle.Draw(new Rect(0, 0, autoCompleteRect.width, autoCompleteRect.height), false, false, false, false);
            }

            //Include scroll view
            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, autoCompleteRect.width, autoCompleteRect.height), scrollPosition, scrollViewContent, false, true);

            Rect elementPos = new Rect(0, 0, autoCompleteRect.width - 20, 20);

            if (Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.DownArrow)
                {
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Tab || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)
                {
                    Clicked(input);
                    Event.current.Use();
                }
            }

            //No match found
            if (matchElements.Count == 0)
            {
                GUI.Label(elementPos, "No match found");
                scrollViewContent.height = 50;
            }
            else
            {
                // NOTE(Juan): If there are too many entries use a diferent strategy than the normal one
                int startingMatchIndex = 0;
                int endMatchIndex = matchElements.Count;
                if (matchElements.Count > 1000)
                {
                    elementPos.y = scrollPosition.y;
                    startingMatchIndex = Mathf.Min(Mathf.FloorToInt(scrollPosition.y / (elementPos.height * 2)), matchElements.Count);
                    endMatchIndex = Mathf.Min(startingMatchIndex + 10, matchElements.Count);
                }

                //Add all the respective buttons for each match entry
                for (int i = startingMatchIndex; i < endMatchIndex; i++)
                {
                    string keyText = matchElements[i];

                    //If this button is clicked set the return value and activate the flag
                    if (GUI.Button(elementPos, keyText))
                    {
                        Clicked(keyText);
                    }

                    if (i == selectedIndex)
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            Color color = GUI.color;
                            GUI.color = selectedColor;
                            selectedStyle.Draw(elementPos, false, false, false, false);
                            GUI.color = color;
                        }
                    }

                    elementPos.y += elementPos.height + 2;
                }

                //Adjust scroll view height
                scrollViewContent.height = matchElements.Count * (elementPos.height + 2);
            }

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        private void Clicked(string keyText)
        {
            selectedIndex = -1;

            returnedContent.text = keyText;
            returnedValue = true;
            changedLastFrame = true;

            Close();
            return;
        }
    }
}
#endif