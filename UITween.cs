using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AV.Animations 
{
    public interface ILerp {}
    public interface ILerp<T> : ILerp
    {
        void SetLerp(T lerp);
    }

    public class UITween : MonoBehaviour, ILerp<float>
    {
        [Serializable] public struct Size
        {
            public Vector2 start;
            public Vector2 end;
        }
        [Serializable] public struct Data
        {
            public Keyframes<Vector2> size;
        }

        public Graphic graphic;
        public CanvasGroup group;
        public Gradient gradient;
        public bool invert;
        public float speed = 1;
        public Size scale;
        public Keyframes<Vector2> sizeFrames;
        public Keyframes<Vector2> scaleFrames;

        RectTransform rect;
        float targetLerp;
        float smoothLerp;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            SetLerp(0);
        }

        private void FixedUpdate()
        {
            smoothLerp = Mathf.Lerp(smoothLerp, targetLerp, Time.unscaledDeltaTime * speed * 60);
            SetLerpRaw(smoothLerp);
        }

        public void SetLerp(float lerp)
        {
            targetLerp = lerp;
        }
        void SetLerpRaw(float lerp)
        {
            if (invert)
                lerp = 1 - lerp;
            if (graphic)
                graphic.color = gradient.Evaluate(lerp);
            if (group)
                group.alpha = lerp;

            sizeFrames.Evaluate(lerp, out var size1, out var size2, out var sizeTime);

            if (size1 != default || size2 != default)
                rect.sizeDelta = Vector2.Lerp(size1, size2, sizeTime);


            //if (size.start != default || size.end != default)
            //    rect.sizeDelta = Vector2.Lerp(size.start, size.end, lerp);

            scaleFrames.Evaluate(lerp, out var scale1, out var scale2, out var scaleTime);

            if (scale1 != default || scale2 != default)
                rect.localScale = Vector2.Lerp(scale1, scale2, scaleTime);
        }
    }
}