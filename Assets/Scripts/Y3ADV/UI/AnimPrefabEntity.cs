using UnityEngine;

namespace Y3ADV
{
    public class AnimPrefabEntity : BaseEntity
    {
        public override Vector3 Position
        {
            get => UntransformVector(transform.localPosition);
            set => transform.localPosition = TransformVector(value);
        }

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector2 v = new Vector2(vec.x * ScreenWidthScalar, vec.y) * 2.0f / 720.0f;
            return new Vector3(v.x, v.y, vec.z);
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector2 v = new Vector2(vec.x, vec.y) * 720.0f / 2.0f;
            v.x /= ScreenWidthScalar;
            return new Vector3(v.x, v.y, vec.z);
        }
    }
}