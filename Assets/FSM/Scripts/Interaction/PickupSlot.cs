using UnityEngine;
using DG.Tweening;

namespace GameInteraction
{
    /// <summary>
    /// Slot that accepts either datatype or variable pickups and provides feedback on success/failure.
    /// </summary>
    public class PickupSlot : MonoBehaviour, IActionButtonProvider
    {
        public enum SlotCategory
        {
            DataType,
            Variable
        }

        [Header("Slot Configuration")]
        [SerializeField] private SlotCategory slotCategory = SlotCategory.DataType;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private PickupSlotDefinition slotDefinition;
        [SerializeField, HideInInspector] private string slotId = "Slot";
        public PickupSlotDefinition SlotDefinition => slotDefinition;
        public string SlotId => slotDefinition != null ? slotDefinition.SlotId : slotId;

        [Header("Interaction Input")]
        [SerializeField] private string requiredButton = "ActionA";

        [Header("Rejection Feedback")]
        [SerializeField, Min(0f)] private float shakeDuration = 0.25f;
        [SerializeField, Min(0f)] private float shakeMagnitude = 0.15f;
        [SerializeField, Min(0f)] private float dropImpulse = 2f;
        [SerializeField] private AudioSource errorAudio;

        [Header("Coordination")]
        [SerializeField] private PickupPuzzleController puzzleController;

        private Interactable currentInteractable;
        private PickupPuzzleMetadata currentMetadata;
        private Tween shakeTween;
        private Vector3 originalLocalPosition;

        private void Awake()
        {
            AutoAssignSnapPoint();
            ResolvePuzzleController();
            SyncSlotId();
            originalLocalPosition = transform.localPosition;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            AutoAssignSnapPoint();
            ResolvePuzzleController();
            SyncSlotId();
        }
#endif

        public bool HasItem => currentInteractable != null;
        public ScriptDataType CurrentDataType => currentMetadata != null ? currentMetadata.EffectiveDataType : ScriptDataType.None;
        public string CurrentVariableRaw => currentMetadata != null ? currentMetadata.EffectiveVariable : string.Empty;
        public string CurrentVariableNormalized => currentMetadata != null ? currentMetadata.EffectiveVariableNormalized : string.Empty;
        public string RequiredButton => requiredButton;

        /// <summary>
        /// Attempts to place the supplied interactable into the slot.
        /// Returns true if accepted; false if rejected.
        /// </summary>
        public bool TryPlace(Interactable interactable)
        {
            if (interactable == null)
            {
                return false;
            }

            if (currentInteractable == interactable)
            {
                return true;
            }

            var metadata = interactable.GetComponent<PickupPuzzleMetadata>();
            if (metadata == null)
            {
                RejectObject(interactable.gameObject, null, playAudio: true);
                return false;
            }

            if (!IsValidForSlot(metadata))
            {
                RejectObject(interactable.gameObject, metadata, playAudio: true);
                return false;
            }

            if (currentInteractable != null)
            {
                RejectCurrent(playAudio: false);
            }

            Accept(interactable, metadata);
            Debug.Log($"PickupSlot '{name}' accepted {DescribePickup(metadata)}");
            puzzleController?.NotifySlotChanged(this);
            return true;
        }

        public void RejectCurrent(bool playAudio = true)
        {
            if (currentInteractable == null)
            {
                return;
            }

            var go = currentInteractable.gameObject;
            Debug.LogWarning($"PickupSlot '{name}' rejected {DescribePickup(currentMetadata)}");
            currentInteractable = null;
            DropToGround(go, playAudio);
            currentMetadata = null;
            puzzleController?.NotifySlotChanged(this);
        }

        private void Accept(Interactable interactable, PickupPuzzleMetadata metadata)
        {
            currentInteractable = interactable;
            currentMetadata = metadata;
            var go = interactable.gameObject;

            ResetShake();

            go.transform.SetParent(snapPoint, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            DisablePhysics(go);
        }

        private bool IsValidForSlot(PickupPuzzleMetadata metadata)
        {
            return slotCategory == SlotCategory.DataType
                ? metadata.HasDataType
                : metadata.HasVariable;
        }

        private void RejectObject(GameObject go, PickupPuzzleMetadata metadata, bool playAudio)
        {
            Debug.LogWarning($"PickupSlot '{name}' rejected {DescribeObject(go, metadata)}");
            DropToGround(go, playAudio);
        }

        private void DropToGround(GameObject go, bool playAudio)
        {
            EnablePhysics(go);
            go.transform.SetParent(null, true);

            if (errorAudio != null && playAudio)
            {
                errorAudio.Play();
            }

            StartShake();

            var rb2D = go.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.AddForce(Vector2.down * dropImpulse, ForceMode2D.Impulse);
            }

            var rb3D = go.GetComponent<Rigidbody>();
            if (rb3D != null)
            {
                rb3D.AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
            }
        }

        private void DisablePhysics(GameObject go)
        {
            var rb2D = go.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.velocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
                rb2D.simulated = false;
            }

            var rb3D = go.GetComponent<Rigidbody>();
            if (rb3D != null)
            {
                rb3D.velocity = Vector3.zero;
                rb3D.angularVelocity = Vector3.zero;
                rb3D.isKinematic = true;
            }

            foreach (var col in go.GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            foreach (var col in go.GetComponents<Collider>())
            {
                col.enabled = false;
            }
        }

        private void EnablePhysics(GameObject go)
        {
            var rb2D = go.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.velocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
                rb2D.simulated = true;
            }

            var rb3D = go.GetComponent<Rigidbody>();
            if (rb3D != null)
            {
                rb3D.velocity = Vector3.zero;
                rb3D.angularVelocity = Vector3.zero;
                rb3D.isKinematic = false;
            }

            foreach (var col in go.GetComponents<Collider2D>())
            {
                col.enabled = true;
            }

            foreach (var col in go.GetComponents<Collider>())
            {
                col.enabled = true;
            }
        }

        private void StartShake()
        {
            if (shakeDuration <= 0f || shakeMagnitude <= 0f)
            {
                return;
            }

            ResetShake();

            shakeTween = transform
                .DOShakePosition(shakeDuration, new Vector3(shakeMagnitude, shakeMagnitude, 0f), 10, 90f, false, true)
                .OnComplete(() =>
                {
                    transform.localPosition = originalLocalPosition;
                    shakeTween = null;
                });
        }

        private void ResetShake()
        {
            if (shakeTween != null)
            {
                shakeTween.Kill();
                shakeTween = null;
            }

            transform.localPosition = originalLocalPosition;
        }

        private void OnDestroy()
        {
            ResetShake();
        }

        private void AutoAssignSnapPoint()
        {
            if (snapPoint != null)
            {
                return;
            }

            var child = transform.Find("SnapPoint");
            snapPoint = child != null ? child : transform;
        }

        private void ResolvePuzzleController()
        {
            if (puzzleController != null)
            {
                return;
            }

            var tagged = GameObject.FindWithTag("PuzzleController");
            if (tagged != null)
            {
                puzzleController = tagged.GetComponent<PickupPuzzleController>();
            }
        }

        private void SyncSlotId()
        {
            if (slotDefinition != null)
            {
                slotId = slotDefinition.SlotId;
            }
        }

        private static string DescribePickup(PickupPuzzleMetadata metadata)
        {
            return DescribeObject(metadata != null ? metadata.gameObject : null, metadata);
        }

        private static string DescribeObject(GameObject go)
        {
            return DescribeObject(go, null);
        }

        private static string DescribeObject(GameObject go, PickupPuzzleMetadata metadata)
        {
            string objectName = go != null ? go.name : "<null>";
            string typeName = metadata != null ? metadata.EffectiveDataType.ToString() : ScriptDataType.None.ToString();
            string variable = metadata != null ? metadata.EffectiveVariableNormalized : string.Empty;
            return string.IsNullOrEmpty(variable)
                ? $"{objectName} [{typeName}]"
                : $"{objectName} [{typeName} {variable}]";
        }
    }
}


