using UnityEngine;

namespace GameInteraction
{
    /// <summary>
    /// Keeps the world scale of a transform constant even if its parent flips or scales.
    /// </summary>
    public class MaintainWorldScale : MonoBehaviour
    {
        private Vector3 desiredWorldScale = Vector3.one;

        public void Initialize(Vector3 worldScale)
        {
            desiredWorldScale = TransformUtilities.NormalizeWorldScale(worldScale);
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            TransformUtilities.SetWorldScale(transform, desiredWorldScale);
        }

        public static MaintainWorldScale Attach(GameObject target, Vector3 worldScale)
        {
            if (target == null)
            {
                return null;
            }

            var component = target.GetComponent<MaintainWorldScale>();
            if (component == null)
            {
                component = target.AddComponent<MaintainWorldScale>();
            }

            component.Initialize(worldScale);
            return component;
        }

        public static void Detach(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var component = target.GetComponent<MaintainWorldScale>();
            if (component != null)
            {
                Destroy(component);
            }
        }
    }
}

