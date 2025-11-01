using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Teleport the agent to a spawn point located by name. Optionally caches the transform on the blackboard.")]
    public class TeleportToSpawnPoint : ActionTask<Transform>
    {
        [Tooltip("Name of the spawn point GameObject to search for. Defaults to 'SpawnPoint'.")]
        public BBParameter<string> spawnPointName = "SpawnPoint";

        [Tooltip("Optional cache for the located spawn point transform.")]
        [BlackboardOnly]
        public BBParameter<Transform> spawnPoint;

        protected override string info
        {
            get
            {
                return $"Teleport To {spawnPointName}";
            }
        }

        protected override void OnExecute()
        {
            Transform target = spawnPoint?.value;

            if (target == null)
            {
                var nameToFind = spawnPointName.value;
                if (!string.IsNullOrEmpty(nameToFind))
                {
                    var found = GameObject.Find(nameToFind);
                    if (found != null)
                    {
                        target = found.transform;
                        if (spawnPoint != null)
                        {
                            spawnPoint.value = target;
                        }
                    }
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"{nameof(TeleportToSpawnPoint)} could not locate a spawn point named '{spawnPointName.value}'.");
                EndAction(false);
                return;
            }

            agent.position = target.position;

            var rb2D = agent.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.velocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
            }

            var rb3D = agent.GetComponent<Rigidbody>();
            if (rb3D != null)
            {
                rb3D.velocity = Vector3.zero;
                rb3D.angularVelocity = Vector3.zero;
            }

            EndAction(true);
        }
    }
}


