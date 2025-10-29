using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Forces landscape-only screen orientation.
    /// Automatically disables all portrait modes.
    /// </summary>
    [Category("✫ Application")]
    [Description("Force landscape orientation (portrait modes disabled)")]
    public class SetScreenOrientationAction : ActionTask
    {
        public enum OrientationMode
        {
            LandscapeAuto,
            LandscapeLeftOnly,
            LandscapeRightOnly
        }

        [Tooltip("Landscape orientation mode")]
        public OrientationMode orientationMode = OrientationMode.LandscapeAuto;

        protected override string info
        {
            get { return string.Format("Set Orientation: {0}", orientationMode); }
        }

        protected override void OnExecute()
        {
            // Always disable portrait modes
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;

            switch (orientationMode)
            {
                case OrientationMode.LandscapeAuto:
                    // Auto-rotate between landscape left and right
                    UnityEngine.Screen.orientation = ScreenOrientation.LandscapeLeft;
                    UnityEngine.Screen.autorotateToLandscapeLeft = true;
                    UnityEngine.Screen.autorotateToLandscapeRight = true;
                    UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
                    break;

                case OrientationMode.LandscapeLeftOnly:
                    // Lock to landscape left
                    UnityEngine.Screen.orientation = ScreenOrientation.LandscapeLeft;
                    break;

                case OrientationMode.LandscapeRightOnly:
                    // Lock to landscape right
                    UnityEngine.Screen.orientation = ScreenOrientation.LandscapeRight;
                    break;
            }

            EndAction(true);
        }
    }
}

