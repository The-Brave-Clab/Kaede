using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class BaseEntity : MonoBehaviour
    {
        public virtual Vector3 Position
        {
            get => UntransformVector(((RectTransform)transform).anchoredPosition3D);
            set => ((RectTransform)transform).anchoredPosition3D = TransformVector(value);
        }

        public Vector2 Pivot
        {
            get => ((RectTransform)transform).pivot;
            set
            {
                RectTransform rt = ((RectTransform)transform);
                if (rt.pivot == value) return;

                Vector2 anchoredPosition = Position;
                var sizeDelta = rt.sizeDelta;
                var pivot = rt.pivot;
                anchoredPosition.x -= sizeDelta.x * pivot.x;
                anchoredPosition.x += sizeDelta.x * value.x;
                anchoredPosition.y -= sizeDelta.y * pivot.y;
                anchoredPosition.y += sizeDelta.y * value.y;
                rt.pivot = value;
                Position = anchoredPosition;
            }
        }

        public static float ScreenWidthScalar => 
            GameSettings.Fixed16By9 ? 
                1.0f :
                Screen.width * 9.0f / 16.0f / Screen.height;
        public virtual float ScaleScalar => 1.0f;

        protected virtual Vector3 TransformVector(Vector3 vec)
        {
            return vec;
        }

        protected virtual Vector3 UntransformVector(Vector3 vec)
        {
            return vec;
        }

        public virtual Color GetColor()
        {
            return Color.clear;
        }

        public virtual void SetColor(Color color)
        {
        }

        private List<Sequence> sequences;
        private Dictionary<string, Sequence> animSequences = null;
        private bool isBeingDestroyed = false;

        protected virtual void Awake()
        {
            sequences = new List<Sequence>();
            animSequences = new Dictionary<string, Sequence>();
        }

        protected virtual void OnDestroy()
        {
            isBeingDestroyed = true;
            StopAllCoroutines();
            foreach (var sequence in sequences)
            {
                sequence.Kill();
            }
        }

        protected Sequence GetSequence()
        {
            Sequence seq = DOTween.Sequence();
            sequences.Add(seq);
            return seq;
        }

        protected void RemoveSequence(Sequence seq)
        {
            sequences.Remove(seq);
        }

        public IEnumerator ColorCommand(Color originalColor, Color targetColor, float duration, Ease ease)
        {
            if (duration == 0)
            {
                SetColor(targetColor);
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Color(originalColor, targetColor, duration, SetColor));
            s.SetEase(ease);

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }
        

        public IEnumerator MoveAnim(Vector3 originalPosition, Vector3 targetPosition, float duration, bool rebound, int loop, Ease ease)
        {
            Sequence seq = GetSequence();
            if (animSequences.ContainsKey("move"))
            {
                var oldSequence = animSequences["move"];
                oldSequence.Kill();
                animSequences.Remove("move");
                RemoveSequence(oldSequence);
            }
            animSequences.Add("move", seq);
            seq.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => Position = value));
            if (rebound)
            {
                seq.Append(DOVirtual.Vector3(targetPosition, originalPosition, duration,
                    value => Position = value));
            }
            int loops = (loop < 0) ? loop : (loop + 1);
            seq.SetLoops(loops, LoopType.Restart);
            seq.SetEase(ease);
            seq.OnComplete(() => { animSequences.Remove("move"); });
            seq.OnKill(() =>
            {
                if (isBeingDestroyed) return;

                Sequence s = GetSequence();
                s.Append(DOVirtual.Vector3(Position, originalPosition, duration,
                    value => Position = value));
                s.OnComplete(() => { RemoveSequence(s); });
            });

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator RotateAnim(float originalAngle, float targetAngle, float duration, bool rebound, int loop, Ease ease)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Sequence seq = GetSequence();
            if (animSequences.ContainsKey("rotate"))
            {
                var oldSequence = animSequences["rotate"];
                oldSequence.Kill(false);
                RemoveSequence(oldSequence);
                animSequences.Remove("rotate");
            }
            animSequences.Add("rotate", seq);
            seq.Append(DOVirtual.Float(originalAngle, targetAngle, duration, 
                value =>
                {
                    eulerAngles.z = value;
                    transform.eulerAngles = eulerAngles;
                }));
            if (rebound)
            {
                seq.Append(DOVirtual.Float(targetAngle, originalAngle, duration,
                    value =>
                    {
                        eulerAngles.z = value;
                        transform.eulerAngles = eulerAngles;
                    }));
            }
            int loops = loop < 0 ? loop : loop + 1;
            seq.SetLoops(loops, LoopType.Restart);
            seq.SetEase(ease);
            seq.OnComplete(() => { animSequences.Remove("rotate"); });
            seq.OnKill(delegate
            {
                if (isBeingDestroyed) return;

                Sequence s = GetSequence();
                s.Append(DOVirtual.Float(targetAngle, originalAngle, duration,
                    value =>
                    {
                        eulerAngles.z = value;
                        transform.eulerAngles = eulerAngles;
                    }));
                s.OnComplete(() => { RemoveSequence(s); });
            });
            
            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator StopAnim(string animName)
        {
            if (animSequences.ContainsKey(animName))
            {
                animSequences[animName].Kill();
                animSequences.Remove(animName);
            }

            yield return null;
        }

        public IEnumerator Move(Vector3 originalPosition, Vector3 targetPosition, float duration, Ease ease)
        {
            if (duration == 0)
            {
                Position = targetPosition;
                yield break;
            }

            Sequence seq = GetSequence();
            seq.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => Position = value));
            seq.SetEase(ease);

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator Rotate(float originalAngle, float targetAngle, float duration, Ease ease)
        {
            var eulerAngles = transform.eulerAngles;
            if (duration == 0)
            {
                eulerAngles.z = targetAngle;
                transform.eulerAngles = eulerAngles;
                yield break;
            }

            Sequence seq = GetSequence();
            seq.Append(DOVirtual.Float(originalAngle, targetAngle, duration,
                value =>
                {
                    eulerAngles.z = value;
                    transform.eulerAngles = eulerAngles;
                }));
            seq.SetEase(ease);

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator Scale(Vector3 originalScale, Vector3 targetScale, float duration, Ease ease)
        {
            if (duration == 0)
            {
                transform.localScale = targetScale;
                yield break;
            }
            
            Sequence seq = GetSequence();
            seq.Append(DOVirtual.Vector3(originalScale, targetScale, duration,
                value => transform.localScale = value));
            seq.SetEase(ease);

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator ColorAlpha(Color color, float fromAlpha, float toAlpha, float duration, bool destroy)
        {
            if (duration <= 0)
            {
                color.a = toAlpha;
                SetColor(color);
            
                if (destroy)
                    Destroy(gameObject);
                
                yield break;
            }

            Sequence seq = GetSequence();
            seq.Append(DOVirtual.Float(fromAlpha, toAlpha, duration,
                value =>
                {
                    color.a = value;
                    SetColor(color);
                }));
            
            if (destroy)
                seq.OnComplete(() => Destroy(gameObject));

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        protected EntityTransform GetTransformState()
        {
            var t = transform;

            return new()
            {
                position = Position,
                angle = t.eulerAngles.z,
                scale = t.localScale.x,
                pivot = Pivot,
                color = GetColor()
            };
        }

        protected void SetTransformState(EntityTransform state)
        {
            var t = transform;

            Position = state.position;
            t.eulerAngles = new Vector3(0, 0, state.angle);
            t.localScale = Vector3.one * state.scale;
            Pivot = state.pivot;
            SetColor(state.color);
        }
    }
}