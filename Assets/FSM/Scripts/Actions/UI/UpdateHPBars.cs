using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/UI")]
    [Description("Updates HP bar visibility based on player HP. Each HP bar represents 1 HP. Animates bars with blink effect when damaged. Uses alpha only (no SetActive) to preserve layout group spacing.")]
    public class UpdateHPBars : ActionTask
    {
        [RequiredField]
        [Tooltip("The HP Parent GameObject that contains all HP Bar children")]
        public BBParameter<GameObject> hpParent;

        [RequiredField]
        [Tooltip("HP Bar prefab to spawn if needed")]
        public BBParameter<GameObject> hpBarPrefab;

        [BlackboardOnly]
        [Tooltip("Current player HP value")]
        public BBParameter<int> playerHP;

        [BlackboardOnly]
        [Tooltip("Maximum HP (used to determine how many bars to spawn)")]
        public BBParameter<int> maxHP;

        [Tooltip("Continuously update every frame (useful if HP changes outside this action)")]
        public BBParameter<bool> continuous = false;

        [UnityEngine.Header("Blink Animation")]
        [Tooltip("Number of times to blink before hiding")]
        [SerializeField] private int blinkCount = 3;
        [Tooltip("Duration of each blink cycle (on + off)")]
        [SerializeField] private float blinkDuration = 0.2f;
        [Tooltip("Minimum alpha during blink (0 = fully transparent)")]
        [SerializeField] private float minAlpha = 0.3f;

        private int lastHP = -1;
        private readonly Dictionary<GameObject, Tween> activeTweens = new Dictionary<GameObject, Tween>();

        protected override string info => $"Update HP Bars ({playerHP}/{maxHP})";

        protected override void OnExecute()
        {
            lastHP = -1;
            UpdateBars();
            if (!continuous.value)
            {
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (continuous.value)
            {
                UpdateBars();
            }
        }

        protected override void OnStop()
        {
            KillAllTweens();
        }

        private void UpdateBars()
        {
            var parent = hpParent.value;
            if (parent == null)
            {
                return;
            }

            int currentHP = playerHP.value;
            int max = maxHP.value;
            int neededBars = Mathf.Max(currentHP, max);

            // Spawn bars if needed
            int currentChildCount = parent.transform.childCount;
            bool spawnedBars = false;
            if (currentChildCount < neededBars && hpBarPrefab.value != null)
            {
                for (int i = currentChildCount; i < neededBars; i++)
                {
                    var instance = UnityEngine.Object.Instantiate(hpBarPrefab.value, parent.transform);
                    instance.name = i == 0 ? "HP Bar" : $"HP Bar ({i})";
                    
                    // Ensure spawned bars are active and set initial alpha
                    if (!instance.activeSelf)
                    {
                        instance.SetActive(true);
                    }
                    var cg = GetOrAddCanvasGroup(instance);
                    cg.alpha = i < currentHP ? 1f : 0f;
                }
                spawnedBars = true;
            }

            // Skip update if HP hasn't changed and we didn't spawn new bars
            if (currentHP == lastHP && !spawnedBars)
            {
                return;
            }

            int childCount = parent.transform.childCount;
            bool tookDamage = currentHP < lastHP;

            // Update bars that should be visible immediately (gained HP or no change)
            for (int i = 0; i < childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                var childGO = child.gameObject;

                // Ensure bar is active (for layout group)
                if (!childGO.activeSelf)
                {
                    childGO.SetActive(true);
                }

                var canvasGroup = GetOrAddCanvasGroup(childGO);

                if (i < currentHP)
                {
                    // Should be visible - stop any animations and set alpha to 1
                    if (activeTweens.ContainsKey(childGO))
                    {
                        activeTweens[childGO].Kill();
                        activeTweens.Remove(childGO);
                    }

                    if (canvasGroup.alpha < 1f)
                    {
                        canvasGroup.alpha = 1f;
                    }
                }
                else if (i >= currentHP && i < lastHP && tookDamage)
                {
                    // This bar needs to disappear - animate it
                    AnimateBarDisappear(childGO);
                }
                else if (i >= currentHP)
                {
                    // Should be hidden - set alpha to 0 immediately
                    if (activeTweens.ContainsKey(childGO))
                    {
                        activeTweens[childGO].Kill();
                        activeTweens.Remove(childGO);
                    }
                    canvasGroup.alpha = 0f;
                }
            }

            lastHP = currentHP;
        }

        private void AnimateBarDisappear(GameObject bar)
        {
            if (bar == null)
            {
                return;
            }

            // Kill any existing tween for this bar
            if (activeTweens.ContainsKey(bar))
            {
                activeTweens[bar].Kill();
            }

            var canvasGroup = GetOrAddCanvasGroup(bar);
            canvasGroup.alpha = 1f;

            // Create blink sequence: fade out and in multiple times, then hide
            Sequence blinkSequence = DOTween.Sequence();
            
            for (int i = 0; i < blinkCount; i++)
            {
                blinkSequence.Append(canvasGroup.DOFade(minAlpha, blinkDuration * 0.5f));
                blinkSequence.Append(canvasGroup.DOFade(1f, blinkDuration * 0.5f));
            }

            // After blinking, fade out completely (alpha = 0, but keep active for layout)
            blinkSequence.Append(canvasGroup.DOFade(0f, blinkDuration * 0.3f));
            blinkSequence.OnComplete(() =>
            {
                if (activeTweens.ContainsKey(bar))
                {
                    activeTweens.Remove(bar);
                }
            });

            activeTweens[bar] = blinkSequence;
        }

        private UnityEngine.CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<UnityEngine.CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<UnityEngine.CanvasGroup>();
            }
            return cg;
        }

        private void KillAllTweens()
        {
            foreach (var tween in activeTweens.Values)
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }
            activeTweens.Clear();
        }
    }
}

