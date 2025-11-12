using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Check if user is logged in. Returns false if logout just happened.")]
    public class CheckIsLoggedIn : ConditionTask
    {
        protected override string info => "Is Logged In?";

        protected override bool OnCheck()
        {
            // If logout just happened, return false to prevent HandleFirstLoginAction from running
            if (LoginManager.HasJustLoggedOut())
            {
                return false;
            }
            return LoginManager.IsLoggedIn();
        }
    }
}

