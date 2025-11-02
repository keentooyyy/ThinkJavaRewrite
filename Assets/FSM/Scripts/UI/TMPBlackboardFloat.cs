using UnityEngine;
using TMPro;
using NodeCanvas.Framework;

namespace GameUI
{
    /// <summary>
    /// Binds a TMP_Text to a variable in the NodeCanvas OWNER Blackboard (the component on the FSM Owner).
    /// Always reads from GraphOwner.blackboard and formats as mm:ss.
    /// </summary>
    [DisallowMultipleComponent]
    public class TMPBlackboardFloat : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("Name of the blackboard float variable (e.g., 'Remaining')")]
        public string variableName = "Remaining";

        [Tooltip("Optional explicit GraphOwner (FSM/BT Owner). If left empty, will search parents.")]
        public GraphOwner graphOwner;

        [Header("Target")] [SerializeField] private TMP_Text target;

        private IBlackboard resolvedBB;

        private void Reset()
        {
            target = GetComponent<TMP_Text>();
        }

        private void Awake()
        {
            if (target == null) target = GetComponent<TMP_Text>();
            ResolveBlackboard();
        }

        private void OnEnable()
        {
            ResolveBlackboard();
            UpdateOnce();
        }

        private void Update()
        {
            UpdateOnce();
        }

        private void ResolveBlackboard()
        {
            // Always and only use the OWNER component blackboard
            if (graphOwner == null)
            {
                graphOwner = GetComponentInParent<GraphOwner>();
            }

            resolvedBB = null;
            if (graphOwner != null)
            {
                resolvedBB = graphOwner.blackboard;
            }
        }

        private void UpdateOnce()
        {
            if (target == null || string.IsNullOrEmpty(variableName)) return;
            if (resolvedBB == null)
            {
                // Try to resolve again if owner was created later
                ResolveBlackboard();
                if (resolvedBB == null) return;
            }

            float val = 0f;
            var varObj = resolvedBB.GetVariable(variableName);
            if (varObj is Variable<float> vf)
            {
                val = vf.value;
            }
            else if (varObj is Variable<int> vi)
            {
                val = vi.value;
            }

            int secs = Mathf.Max(0, Mathf.CeilToInt(val));
            int m = secs / 60;
            int s = secs % 60;
            target.text = string.Format("{0:00}:{1:00}", m, s);
        }
    }
}
