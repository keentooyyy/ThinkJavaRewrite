using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Enemy")]
    [Description("Always returns false - waypoint switching is handled automatically in MoveEnemyBetweenWaypoints")]
    public class CheckReachedWaypoint : ConditionTask
    {
        protected override string info
        {
            get { return "Reached Waypoint? (Not Used)"; }
        }

        protected override bool OnCheck()
        {
            // Waypoint switching is handled automatically in MoveEnemyBetweenWaypoints
            // This condition is kept for backwards compatibility but always returns false
            return false;
        }
    }
}

