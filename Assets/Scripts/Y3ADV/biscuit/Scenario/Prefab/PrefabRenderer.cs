using UnityEngine;

namespace Y3ADV.Scenario
{
    [RequireComponent(typeof(Camera))]
    public class PrefabRenderer : MonoBehaviour
    {
        private Camera renderCamera;
        public Transform prefabPosAnchor;

        private void Awake()
        {
            renderCamera = GetComponent<Camera>();
        }

        public GameObject Init(GameObject prefab, string objectName, Vector2 pos, float scale)
        {
            GameObject instantiated = Instantiate(prefab, renderCamera.transform);
            instantiated.transform.localPosition = new Vector3(pos.x, pos.y, prefabPosAnchor.position.z);
            instantiated.transform.localScale = Vector3.one * scale;
            instantiated.name = objectName;
            return instantiated;
        }
    }
}
