using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Check if user is logged in")]
    public class CheckIsLoggedIn : ConditionTask
    {
        protected override string info => "Is Logged In?";

        protected override bool OnCheck()
        {
            return LoginManager.IsLoggedIn();
        }
    }
}

