using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Quits the application or stops play mode in editor.
    /// Works properly in both Unity Editor and builds.
    /// </summary>
    [Category("Application")]
    [Description("Quit the game (stops play mode in editor, closes app in build)")]
    public class QuitGameAction : ActionTask
    {
        [Tooltip("Optional delay in seconds before quitting")]
        public BBParameter<float> delay = 0f;

        private float elapsedTime = 0f;

        protected override string info
        {
            get 
            { 
                if (delay.value > 0)
                    return string.Format("Quit Game (after {0}s)", delay.value);
                return "Quit Game";
            }
        }

        protected override void OnExecute()
        {
            if (delay.value <= 0)
            {
                QuitGame();
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (delay.value > 0)
            {
                elapsedTime += Time.deltaTime;
                
                if (elapsedTime >= delay.value)
                {
                    QuitGame();
                    EndAction(true);
                }
            }
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

