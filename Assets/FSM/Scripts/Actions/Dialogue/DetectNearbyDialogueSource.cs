using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DialogueRuntime;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Dialogue")]
    [Description("Detect nearby DialogueSource components using proximity check")]
    public class DetectNearbyDialogueSource : ActionTask<Transform>
    {
        [BlackboardOnly]
        [Tooltip("Store the nearby DialogueSource GameObject here")]
        public BBParameter<GameObject> nearbyDialogueSource;

        [Tooltip("Detection radius")]
        public BBParameter<float> detectionRadius = 1.5f;
        
        [Tooltip("Layer mask for dialogue sources")]
        public LayerMask dialogueLayer;
        
        [Tooltip("Tag filter (optional, leave empty to detect all)")]
        public BBParameter<string> filterTag = "";

        protected override string info => "Detect Nearby Dialogue Source";

        protected override void OnUpdate()
        {
            int mask = dialogueLayer.value != 0 ? dialogueLayer : ~0;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                agent.position,
                detectionRadius.value,
                mask
            );

            GameObject closest = null;
            float closestDistance = Mathf.Infinity;

            foreach (var col in colliders)
            {
                if (!string.IsNullOrEmpty(filterTag.value) && !col.CompareTag(filterTag.value))
                {
                    continue;
                }

                var dialogueSource = col.GetComponentInParent<DialogueSource>();
                if (dialogueSource == null || !dialogueSource.HasDialogue)
                {
                    continue;
                }

                float distance = Vector2.Distance(agent.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closest = dialogueSource.gameObject;
                    closestDistance = distance;
                }
            }

            nearbyDialogueSource.value = closest;
        }

        protected override void OnStop()
        {
            nearbyDialogueSource.value = null;
        }

        public override void OnDrawGizmosSelected()
        {
            if (agent != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(agent.position, detectionRadius.value);
            }
        }
    }
}

