using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Check if user has cloud save enabled (logged in)")]
    public class CheckHasCloudSave : ConditionTask
    {
        protected override string info => "Has Cloud Save?";

        protected override bool OnCheck()
        {
            return LoginManager.HasCloudSave();
        }
    }
}

