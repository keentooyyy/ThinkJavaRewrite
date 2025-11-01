using UnityEngine;

namespace GameInteraction
{
    public static class TransformUtilities
    {
        public static Vector3 NormalizeWorldScale(Vector3 scale)
        {
            return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }

        public static void SetWorldScale(Transform target, Vector3 worldScale)
        {
            if (target == null)
            {
                return;
            }

            target.localScale = ComputeLocalScaleForParent(target.parent, worldScale);
        }

        public static Vector3 ComputeLocalScaleForParent(Transform parent, Vector3 desiredWorldScale)
        {
            if (parent == null)
            {
                return desiredWorldScale;
            }

            Vector3 parentScale = parent.lossyScale;
            return new Vector3(
                SafeDivide(desiredWorldScale.x, parentScale.x),
                SafeDivide(desiredWorldScale.y, parentScale.y),
                SafeDivide(desiredWorldScale.z, parentScale.z)
            );
        }

        public static float SafeDivide(float numerator, float denominator)
        {
            return Mathf.Approximately(denominator, 0f) ? 0f : numerator / denominator;
        }
    }
}

