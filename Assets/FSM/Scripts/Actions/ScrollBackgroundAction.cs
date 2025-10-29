using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Scrolls a background horizontally and automatically repositions it when it goes off screen.
    /// Sprite width is auto-detected from the SpriteRenderer component.
    /// Camera-aware: teleports only when completely out of camera view.
    /// </summary>
    [Category("Movement/Direct")]
    [Description("Scrolls a background horizontally and repositions it when it goes off screen. Camera-aware.")]
    public class ScrollBackgroundAction : ActionTask<Transform>
    {
        // Configuration
        [Tooltip("Speed of scrolling (negative = scroll left, positive = scroll right)")]
        public BBParameter<float> scrollSpeed = -2f;
        
        [Tooltip("Number of background copies in the scene (usually 3 for seamless infinite scroll)")]
        public BBParameter<int> backgroundCount = 3;
        
        [Tooltip("Overlap in pixels to prevent gaps between backgrounds (recommended: 2-5)")]
        public BBParameter<float> overlapPixels = 2f;

        // Runtime values
        private Camera mainCamera;
        private float spriteWidth;
        private float effectiveWidth;
        private float cameraLeftEdge;

        protected override string info
        {
            get { return string.Format("Scroll at {0}/sec", scrollSpeed); }
        }

        protected override string OnInit()
        {
            var spriteRenderer = agent.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return "ScrollBackgroundAction requires a SpriteRenderer component!";
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return "No main camera found in scene!";
            }

            spriteWidth = spriteRenderer.bounds.size.x;
            
            float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
            float overlapWorldUnits = overlapPixels.value / pixelsPerUnit;
            
            // Effective width accounts for overlap between backgrounds
            effectiveWidth = spriteWidth - overlapWorldUnits;
            
            return null;
        }

        protected override void OnUpdate()
        {
            agent.position += new Vector3(scrollSpeed.value * Time.deltaTime, 0, 0);
            
            // Get camera's left edge in world space
            Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, agent.position.z - mainCamera.transform.position.z));
            cameraLeftEdge = cameraBottomLeft.x;
            
            // Teleport when sprite's right edge goes past camera's left edge
            float spriteRightEdge = agent.position.x + (spriteWidth * 0.5f);
            if (spriteRightEdge < cameraLeftEdge)
            {
                // Move this background forward by the total loop distance
                // This positions it right after the last background
                float jumpDistance = effectiveWidth * backgroundCount.value;
                
                Vector3 newPos = agent.position;
                newPos.x += jumpDistance;
                agent.position = newPos;
            }
        }
    }
}