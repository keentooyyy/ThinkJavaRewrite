using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Check if this is the first login")]
    public class CheckFirstLogin : ConditionTask
    {
        protected override string info => "Is First Login?";

        protected override bool OnCheck()
        {
            return GameSaveManager.IsFirstLogin();
        }
    }
}

