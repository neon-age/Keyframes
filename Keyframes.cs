using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AV.Animations 
{
    [Serializable] public struct Keyframes<T> : ISerializationCallbackReceiver
    {
        [Serializable] public struct Frame
        {
            public float time;
            public T value;
        }
        public Frame[] frames;
        public int Count => frames.Length;

        public ref Frame this[int i] => ref frames[i];

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // Do not sort frames by time while dragging!
            if (KeyframesDrawer.Drag) return;
            
            frames = frames?.OrderBy(x => x.time).ToArray();
#endif
        }
        public void OnAfterDeserialize() { }


        public void Evaluate(float lerp, out T v1, out T v2, out float t)
        {
            v1 = v2 = default;
            float t1 = 0, t2 = 0;

            var count = frames.Length;

            for (int i = 0; i < count; i++)
            {
                ref var f = ref frames[i];
                if (lerp >= f.time) { t1 = f.time; v1 = f.value; }
            }
            for (int i = 0; i < count; i++)
            {
                ref var f = ref frames[i];
                if (lerp <= f.time) { t2 = f.time; v2 = f.value; break; }
            }
            if (count > 0)
            {
                ref var f = ref frames[count - 1];
                if (lerp >= f.time) { t2 = f.time; v2 = f.value; }
            }

            t = (lerp - t1) / (t2 - t1);
            if (float.IsNaN(t))
                t = 0;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Keyframes<>))]
    class KeyframesDrawer : PropertyDrawer
    {
        class Data
        {
            public int selected = -1;
            public float itemHeight;
            public float dragTime = -1;
            public bool drag;
            public bool mouseUp;
            public SerializedProperty currFrame;
        }
        static Texture keyframeIcon = EditorGUIUtility.IconContent("blendKeySelected").image;
        static Dictionary<string, Data> datas = new Dictionary<string, Data>();
        internal static bool Drag;
       
        internal static string FocusedProp;

        public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label)
        {
            var evt = Event.current;
            var path = prop.propertyPath;
            var mousePos = evt.mousePosition;
            EditorGUI.BeginProperty(r, label, prop);
            EditorGUI.LabelField(new Rect(r) { height = 18 }, label);

            r.y += 20;
            r.xMin += 3; r.xMax -= 5;
            EditorGUI.DrawRect(new Rect(r) { height = 4 }, new Color(0, 0, 0, 0.3f));
            r.xMin -= 5; r.xMax -= 5;

            
            if (!datas.TryGetValue(path, out var data))
                datas.Add(path, data = new Data());

            var frames = P(prop, "frames");
            var arraySize = frames.arraySize;
            data.selected = Mathf.Clamp(data.selected, -1, arraySize - 1);

            var tr = new Rect(r) { height = 16 }; tr.y -= 5; tr.xMin = 0; tr.xMax += 30;
            if (tr.Contains(mousePos))
            {
                if (evt.clickCount == 2)
                {
                    frames.InsertArrayElementAtIndex(arraySize);
                    P(frames, arraySize, "time").floatValue = (mousePos.x - r.x) / (r.xMax - r.x);
                }
                if (evt.type == EventType.MouseDrag){
                    data.drag = Drag = true;
                    FocusedProp = path;
                }
            }

            if (data.mouseUp){
                data.mouseUp = false;

                // Find selected frame after OrderBy time
                for (int i = 0; i < arraySize; i++)
                    if (Math.Abs(data.dragTime - P(frames, i, "time").floatValue) < 0.001f)
                    {
                        data.selected = i;
                        Debug.Log(i);
                        break;
                    }
            }

            for (int i = arraySize - 1; i >= 0; i--)
            {
                var frame = P(frames, i);
                var time = P(frame, "time");

                var kr = new Rect(r) { width = 19, height = 19 };
                kr.x = Mathf.Lerp(r.x, r.xMax, time.floatValue);
                kr.y -= 6;

                if (kr.Contains(mousePos) && evt.button == 0 && evt.type == EventType.MouseDown)
                {
                    data.selected = i;
                    FocusedProp = path;
                    Repaint(prop);
                }
                var selected = i == data.selected && FocusedProp == path;

                if (selected && data.drag)
                {
                    //float xMin = r.x + 16;
                    //data.dragTime = time.floatValue = Mathf.Clamp01((mousePos.x - xMin) / (r.xMax - xMin));
                    data.dragTime = time.floatValue = Mathf.Clamp01(evt.delta.x / Screen.width + time.floatValue);
                    prop.serializedObject.ApplyModifiedProperties();
                    Repaint(prop);
                }
                if (selected && evt.type == EventType.KeyDown && 
                    (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace))
                {
                    data.selected = -1;
                    frames.DeleteArrayElementAtIndex(i);
                }

                GUI.color = selected ? new Color32(117, 173, 255, 255) : new Color32(175, 175, 175, 255);
                GUI.Box(kr, keyframeIcon, EditorStyles.label);
                GUI.color = Color.white;
            }

            // End drag
            if (data.drag && evt.type == EventType.MouseUp)
            {
                data.drag = Drag = false;
                data.mouseUp = true;

                prop.serializedObject.ApplyModifiedProperties();
                Repaint(prop);
            }

            // Draw selected frame data
            if (data.selected != -1)
            {
                data.currFrame = P(frames, data.selected);
                DrawFrame(r, data.currFrame, data);
            }
            EditorGUI.EndProperty();
        }

        void DrawFrame(Rect r, SerializedProperty frame, Data data)
        {
            var time = P(frame, "time");
            var value = P(frame, "value");

            r.y += 10; r.xMax += 10;
            value.isExpanded = true;

            EditorGUI.Slider(new Rect(r) { height = 18 }, time, 0, 1);
            if (GUI.changed)
                KeyframesDrawer.Drag = true;

            var itemRect = new Rect(r);
            itemRect.y += 20;

            data.itemHeight = 0;

            var iterator = value.Copy();
            var enterChildren = iterator.hasVisibleChildren;
            var endProperty = iterator.GetEndProperty();

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                data.itemHeight += itemRect.height = EditorGUI.GetPropertyHeight(iterator, false);

                enterChildren = EditorGUI.PropertyField(itemRect, iterator, enterChildren);

                itemRect.y += itemRect.height + 2;
            }
        }

        static SerializedProperty P(SerializedProperty p, int i) => p.GetArrayElementAtIndex(i);
        static SerializedProperty P(SerializedProperty p, string n) => p.FindPropertyRelative(n);
        static SerializedProperty P(SerializedProperty p, int i, string n) => p.GetArrayElementAtIndex(i).FindPropertyRelative(n);

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (!datas.TryGetValue(prop.propertyPath, out var data) || data.selected == -1 || data.currFrame == null)
                return 36;
            return 56 + data.itemHeight;
        }

        static void Repaint(SerializedProperty prop)
        {
            var obj = prop.serializedObject;
            foreach (var i in ActiveEditorTracker.sharedTracker.activeEditors)
                if (i.serializedObject == obj) { i.Repaint(); return; }
        }
    }
#endif 
}



