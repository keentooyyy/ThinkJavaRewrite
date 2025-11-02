using UnityEngine;
using DG.Tweening;
using GameInput;

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
        [SerializeField] private InputConfig inputConfig;
        [ButtonName("inputConfig")]
        
        [SerializeField] private string requiredButton = "ActionA";

        [Header("Rejection Feedback")]
        [SerializeField, Min(0f)] private float shakeDuration = 0.25f;
        [SerializeField, Min(0f)] private float shakeMagnitude = 0.15f;
        [SerializeField, Min(0f)] private float dropImpulse = 2f;
        [SerializeField] private AudioSource errorAudio;

        [Header("Coordination")]
        [SerializeField] private PickupPuzzleController puzzleController;

        [Header("Drop Animation")]
        [SerializeField] private bool tweenDrop = false;
        [SerializeField, Min(0f)] private float dropTweenDuration = 0.2f;
        [SerializeField] private Vector3 dropTweenOffset = new Vector3(0f, -0.25f, 0f);
        [SerializeField] private Ease dropTweenEase = Ease.InQuad;

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
        public GameObject CurrentObject => currentInteractable != null ? currentInteractable.gameObject : null;

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
                var existing = currentInteractable.gameObject;
                ResetState(false);
                EnablePhysics(existing);
                existing.transform.SetParent(null, true);
                TransformUtilities.SetWorldScale(existing.transform, TransformUtilities.NormalizeWorldScale(existing.transform.lossyScale));
                MaintainWorldScale.Detach(existing);
            }

            Accept(interactable, metadata);
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
            currentInteractable = null;
            DropToGround(go, playAudio);
            currentMetadata = null;
            puzzleController?.NotifySlotChanged(this);
        }

        /// <summary>
        /// Releases the current item without playing rejection feedback so it can be picked up again.
        /// Returns the released GameObject or null if empty.
        /// </summary>
        public GameObject RetrieveCurrent(bool playAudio = false)
        {
            if (currentInteractable == null)
            {
                return null;
            }

            var go = currentInteractable.gameObject;
            ResetState(playAudio);
            EnablePhysics(go);
            go.transform.SetParent(null, true);
            TransformUtilities.SetWorldScale(go.transform, TransformUtilities.NormalizeWorldScale(go.transform.lossyScale));
            MaintainWorldScale.Detach(go);
            puzzleController?.NotifySlotChanged(this);
            return go;
        }

        private void Accept(Interactable interactable, PickupPuzzleMetadata metadata)
        {
            currentInteractable = interactable;
            currentMetadata = metadata;
            var go = interactable.gameObject;

            ResetShake();

            Vector3 worldScale = TransformUtilities.NormalizeWorldScale(go.transform.lossyScale);
            Quaternion worldRotation = go.transform.rotation;

            go.transform.SetParent(snapPoint, true);
            TransformUtilities.SetWorldScale(go.transform, worldScale);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.rotation = worldRotation;

            MaintainWorldScale.Attach(go, worldScale);

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
            DropToGround(go, playAudio, startFromSnapPoint: true);
        }

        private void DropToGround(GameObject go, bool playAudio, bool startFromSnapPoint = false)
        {
            go.transform.SetParent(null, true);
            TransformUtilities.SetWorldScale(go.transform, TransformUtilities.NormalizeWorldScale(go.transform.lossyScale));
            MaintainWorldScale.Detach(go);

            if (startFromSnapPoint && snapPoint != null)
            {
                go.transform.position = snapPoint.position;
                go.transform.rotation = snapPoint.rotation;
            }

            if (errorAudio != null && playAudio)
            {
                errorAudio.Play();
            }

            StartShake();

            var rb2D = go.GetComponent<Rigidbody2D>();
            var rb3D = go.GetComponent<Rigidbody>();

            if (tweenDrop && dropTweenDuration > 0f)
            {
                var startPos = go.transform.position;
                var targetPos = startPos + dropTweenOffset;
                if (rb2D != null)
                {
                    rb2D.simulated = false;
                    rb2D.velocity = Vector2.zero;
                }

                if (rb3D != null)
                {
                    rb3D.isKinematic = true;
                    rb3D.velocity = Vector3.zero;
                }

                Sequence seq = DOTween.Sequence();
                seq.Append(go.transform.DOMove(targetPos, dropTweenDuration).SetEase(dropTweenEase));
                seq.OnComplete(() =>
                {
                    EnablePhysics(go);
                    // Apply a small downward impulse if configured
                    if (rb2D != null && dropImpulse > 0f)
                    {
                        rb2D.AddForce(Vector2.down * dropImpulse, ForceMode2D.Impulse);
                    }
                    if (rb3D != null && dropImpulse > 0f)
                    {
                        rb3D.AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
                    }
                });
            }
            else
            {
                EnablePhysics(go);
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                    if (dropImpulse > 0f)
                    {
                        rb2D.AddForce(Vector2.down * dropImpulse, ForceMode2D.Impulse);
                    }
                }
                if (rb3D != null)
                {
                    rb3D.velocity = Vector3.zero;
                    if (dropImpulse > 0f)
                    {
                        rb3D.AddForce(Vector3.down * dropImpulse, ForceMode.Impulse);
                    }
                }
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

        private void ResetState(bool playAudio)
        {
            ResetShake();
            if (playAudio && errorAudio != null)
            {
                errorAudio.Play();
            }

            currentInteractable = null;
            currentMetadata = null;
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

