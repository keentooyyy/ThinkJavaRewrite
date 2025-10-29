using System;
using UnityEngine;

namespace TouchControls
{
    /// <summary>
    /// Static manager for touch input events.
    /// Fires events when directional buttons are pressed/released.
    /// </summary>
    public static class TouchInputManager
    {
        public enum Direction { None, Up, Down, Left, Right }

        // Events
        public static event Action<Direction> OnDirectionPressed;
        public static event Action<Direction> OnDirectionReleased;
        public static event Action OnJumpPressed;
        public static event Action OnJumpReleased;

        // Current state
        private static Direction currentDirection = Direction.None;
        private static bool isJumping = false;
        private static bool jumpPressedThisFrame = false;

        /// <summary>
        /// Get current horizontal input (-1 = left, 0 = none, 1 = right)
        /// </summary>
        public static float HorizontalAxis()
        {
            if (currentDirection == Direction.Left) return -1f;
            if (currentDirection == Direction.Right) return 1f;
            return 0f;
        }

        /// <summary>
        /// Get current vertical input (-1 = down, 0 = none, 1 = up)
        /// </summary>
        public static float VerticalAxis()
        {
            if (currentDirection == Direction.Down) return -1f;
            if (currentDirection == Direction.Up) return 1f;
            return 0f;
        }

        /// <summary>
        /// Is jump currently pressed?
        /// </summary>
        public static bool IsJumping()
        {
            return isJumping;
        }

        /// <summary>
        /// Get and consume jump pressed this frame (for single-tap detection)
        /// Returns true only once per press, then resets to false
        /// </summary>
        public static bool GetJumpPressed()
        {
            bool result = jumpPressedThisFrame;
            jumpPressedThisFrame = false; // Consume the press
            return result;
        }

        /// <summary>
        /// Called by VirtualButton when a direction is pressed
        /// </summary>
        public static void PressDirection(Direction direction)
        {
            if (direction == Direction.None) return;

            // Release current direction if different
            if (currentDirection != Direction.None && currentDirection != direction)
            {
                ReleaseDirection(currentDirection);
            }

            if (currentDirection != direction)
            {
                currentDirection = direction;
                OnDirectionPressed?.Invoke(direction);
            }
        }

        /// <summary>
        /// Called by VirtualButton when a direction is released
        /// </summary>
        public static void ReleaseDirection(Direction direction)
        {
            if (currentDirection == direction)
            {
                currentDirection = Direction.None;
                OnDirectionReleased?.Invoke(direction);
            }
        }

        /// <summary>
        /// Called by VirtualButton when jump is pressed
        /// </summary>
        public static void PressJump()
        {
            if (!isJumping)
            {
                isJumping = true;
                jumpPressedThisFrame = true; // Set the frame flag
                OnJumpPressed?.Invoke();
            }
        }

        /// <summary>
        /// Called by VirtualButton when jump is released
        /// </summary>
        public static void ReleaseJump()
        {
            if (isJumping)
            {
                isJumping = false;
                OnJumpReleased?.Invoke();
            }
        }

        /// <summary>
        /// Clear all input (useful for scene transitions)
        /// </summary>
        public static void Clear()
        {
            currentDirection = Direction.None;
            isJumping = false;
            jumpPressedThisFrame = false;
        }
    }
}

