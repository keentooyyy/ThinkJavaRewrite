using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.StateMachines;

namespace GameEnemies
{
    /// <summary>
    /// Global enemy manager with a single FSM that controls all enemies.
    /// Enemies register themselves automatically.
    /// </summary>
    [AddComponentMenu("Game/Global Enemy Manager")]
    public class GlobalEnemyManager : FSMOwner
    {
        public static GlobalEnemyManager Instance { get; private set; }

        private List<Enemy> enemies = new List<Enemy>();

        /// <summary>
        /// Get all registered enemies
        /// </summary>
        public List<Enemy> Enemies => enemies;

        /// <summary>
        /// Get all alive enemies
        /// </summary>
        public List<Enemy> AliveEnemies
        {
            get
            {
                var alive = new List<Enemy>();
                foreach (var enemy in enemies)
                {
                    if (enemy == null)
                    {
                        Debug.LogWarning("[GLOBAL_ENEMY_MANAGER] Found NULL enemy in list!");
                        continue;
                    }
                    
                    if (!enemy.IsDead)
                    {
                        alive.Add(enemy);
                    }
                }
                return alive;
            }
        }

        protected new void Awake()
        {
            base.Awake();
            
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[GLOBAL_ENEMY_MANAGER] Duplicate instance detected! Destroying {gameObject.name}");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            enemies.Clear(); // Clear enemies list on scene reload
        }

        protected new void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            base.OnDestroy();
        }

        /// <summary>
        /// Register an enemy (called automatically by Enemy component)
        /// </summary>
        public void RegisterEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[GLOBAL_ENEMY_MANAGER] RegisterEnemy called with NULL enemy!");
                return;
            }

            if (enemies.Contains(enemy))
            {
                Debug.LogWarning($"[GLOBAL_ENEMY_MANAGER] Enemy {enemy.gameObject.name} is already registered!");
                return;
            }

            enemies.Add(enemy);
        }

        /// <summary>
        /// Unregister an enemy (called automatically when enemy is destroyed)
        /// </summary>
        public void UnregisterEnemy(Enemy enemy)
        {
            if (enemy != null)
            {
                enemies.Remove(enemy);
            }
        }

        /// <summary>
        /// Check if any enemy was stomped (doesn't reset flags)
        /// </summary>
        public bool AnyEnemyStomped()
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead && enemy.HasBeenStomped)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the first stomped enemy (if any) and reset its flag
        /// </summary>
        public Enemy GetStompedEnemy()
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead && enemy.WasStomped)
                {
                    return enemy;
                }
            }
            return null;
        }
    }
}

