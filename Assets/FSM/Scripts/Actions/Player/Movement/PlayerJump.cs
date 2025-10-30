using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Make player jump")]
    public class PlayerJump : ActionTask<Rigidbody2D>
    {
        public BBParameter<float> jumpForce = 7f;

        protected override string info
        {
            get { return $"Jump (Force: {jumpForce})"; }
        }

        protected override void OnExecute()
        {
            agent.velocity = new Vector2(agent.velocity.x, jumpForce.value);
            EndAction(true);
        }
    }
}

