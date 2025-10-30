using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Player")]
    [Description("Check if player HP meets a condition")]
    public class CheckPlayerHP : ConditionTask
    {
        public enum ComparisonType
        {
            LessThan,
            LessThanOrEqual,
            Equal,
            GreaterThanOrEqual,
            GreaterThan
        }

        [BlackboardOnly]
        public BBParameter<int> playerHP;

        public BBParameter<int> compareValue = 0;
        public ComparisonType comparison = ComparisonType.LessThanOrEqual;

        protected override string info
        {
            get { return $"HP {comparison} {compareValue}"; }
        }

        protected override bool OnCheck()
        {
            switch (comparison)
            {
                case ComparisonType.LessThan:
                    return playerHP.value < compareValue.value;
                case ComparisonType.LessThanOrEqual:
                    return playerHP.value <= compareValue.value;
                case ComparisonType.Equal:
                    return playerHP.value == compareValue.value;
                case ComparisonType.GreaterThanOrEqual:
                    return playerHP.value >= compareValue.value;
                case ComparisonType.GreaterThan:
                    return playerHP.value > compareValue.value;
                default:
                    return false;
            }
        }
    }
}

