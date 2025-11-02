using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Input")]
public class NC_PressDown : ConditionTask
{
    public float deadzone = 0.2f;
    protected override bool OnCheck()
    {
        return Input.GetAxisRaw("Vertical") < -deadzone;
    }
}
