using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class SpriteImage : BaseEntity, IStateSavable<CommonResourceState>
    {
        public string resourceName;

        private Image image = null;
        private RectTransform rectTransform = null;

        protected override void Awake()
        {
            base.Awake();
            image = GetComponent<Image>();
            rectTransform = (RectTransform) transform;
        }

        public override Color GetColor()
        {
            return image.color;
        }

        public override void SetColor(Color color)
        {
            image.color = color;
        }

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x * ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x / ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        public CommonResourceState GetState()
        {
            return new()
            {
                name = gameObject.name,
                resourceName = resourceName,

                transform = GetTransformState()
            };
        }

        public IEnumerator RestoreState(CommonResourceState state)
        {
            if (name != state.name || resourceName != state.resourceName)
            {
                Debug.LogError("Applying state to wrong sprite!");
                yield break;
            }

            RestoreTransformState(state.transform);
        }
    }
}