using System;
using System.Collections.Generic;
using UnityEngine;
using GameInput;

namespace GameState
{
    public enum GameFreezeType
    {
        None = 0,
        Dialogue = 1,
        Full = 2,
    }

    /// <summary>
    /// Centralized helper for pausing gameplay while allowing selective inputs.
    /// Dialogue freeze pauses gameplay time but lets whitelisted buttons through.
    /// Full freeze pauses time scale and ignores all gameplay buttons.
    /// </summary>
    public static class GameFreezeManager
    {
        public static event Action<GameFreezeType> OnFreezeChanged;

        private static GameFreezeType currentFreeze = GameFreezeType.None;
        private static float storedTimeScale = 1f;
        private static float storedFixedDeltaTime = 0.02f;
        private static bool timeScaleOverridden;

        private static readonly HashSet<string> dialogueAllowedButtons =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ActionA", "ActionB" };

        private static readonly HashSet<string> fullAllowedButtons =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static GameFreezeManager()
        {
            storedTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            storedFixedDeltaTime = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 0.02f;
        }

        public static GameFreezeType CurrentFreeze => currentFreeze;
        public static bool IsFrozen => currentFreeze != GameFreezeType.None;
        public static bool IsDialogueFreeze => currentFreeze == GameFreezeType.Dialogue;
        public static bool IsFullFreeze => currentFreeze == GameFreezeType.Full;
        public static bool AllowsMovementInput => currentFreeze == GameFreezeType.None;
        public static bool AllowsGameplayUpdate => currentFreeze == GameFreezeType.None;

        public static void SetFreeze(GameFreezeType freezeType)
        {
            if (freezeType == currentFreeze)
            {
                return;
            }

            bool wasFullFreeze = currentFreeze == GameFreezeType.Full;

            currentFreeze = freezeType;

            if (currentFreeze == GameFreezeType.Full)
            {
                ApplyTimeScaleFreeze();
            }
            else if (wasFullFreeze)
            {
                RestoreTimeScale();
            }

            InputManager.Clear();
            OnFreezeChanged?.Invoke(currentFreeze);
        }

        public static void ClearFreeze()
        {
            SetFreeze(GameFreezeType.None);
        }

        public static bool IsButtonAllowed(string buttonName)
        {
            if (string.IsNullOrWhiteSpace(buttonName))
            {
                return true;
            }

            switch (currentFreeze)
            {
                case GameFreezeType.None:
                    return true;
                case GameFreezeType.Dialogue:
                    return dialogueAllowedButtons.Contains(buttonName);
                case GameFreezeType.Full:
                    return fullAllowedButtons.Contains(buttonName);
                default:
                    return true;
            }
        }

        public static void ConfigureDialogueWhitelist(IEnumerable<string> buttonNames)
        {
            UpdateWhitelist(dialogueAllowedButtons, buttonNames);
        }

        public static void ConfigureFullWhitelist(IEnumerable<string> buttonNames)
        {
            UpdateWhitelist(fullAllowedButtons, buttonNames);
        }

        private static void ApplyTimeScaleFreeze()
        {
            if (!timeScaleOverridden)
            {
                storedTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
                storedFixedDeltaTime = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 0.02f;
                timeScaleOverridden = true;
            }

            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
        }

        private static void RestoreTimeScale()
        {
            Time.timeScale = storedTimeScale;
            Time.fixedDeltaTime = storedFixedDeltaTime;
            timeScaleOverridden = false;
        }

        private static void UpdateWhitelist(HashSet<string> target, IEnumerable<string> source)
        {
            target.Clear();

            if (source == null)
            {
                return;
            }

            foreach (var name in source)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                target.Add(name.Trim());
            }
        }
    }
}
