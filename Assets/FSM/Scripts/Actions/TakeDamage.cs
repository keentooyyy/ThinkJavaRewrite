using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Decrease player HP and trigger iframes")]
    public class TakeDamage : ActionTask
    {
        [BlackboardOnly]
        public BBParameter<int> playerHP;

        public BBParameter<int> damageAmount = 1;

        [BlackboardOnly]
        public BBParameter<bool> isInvincible;

        protected override string info
        {
            get { return $"Take {damageAmount} Damage"; }
        }

        protected override void OnExecute()
        {
            if (!isInvincible.value)
            {
                playerHP.value -= damageAmount.value;
                isInvincible.value = true;
            }
            EndAction(true);
        }
    }
}

