using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Player")]
    [Description("Check if player is currently invincible")]
    public class CheckInvincibility : ConditionTask
    {
        [BlackboardOnly]
        public BBParameter<bool> isInvincible;

        public bool checkIfInvincible = false;

        protected override string info
        {
            get { return checkIfInvincible ? "Is Invincible?" : "Not Invincible?"; }
        }

        protected override bool OnCheck()
        {
            return isInvincible.value == checkIfInvincible;
        }
    }
}
