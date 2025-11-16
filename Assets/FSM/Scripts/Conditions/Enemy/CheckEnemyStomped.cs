using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameEnemies;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Enemy")]
    [Description("Check if any enemy was stomped on by player (Mario-style)")]
    public class CheckEnemyStomped : ConditionTask
    {
        protected override string info
        {
            get { return "Any Enemy Stomped?"; }
        }

        protected override bool OnCheck()
        {
            var manager = GlobalEnemyManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[CHECK_STOMPED] GlobalEnemyManager.Instance is NULL!");
                return false;
            }

            bool result = manager.AnyEnemyStomped();
            return result;
        }
    }
}

