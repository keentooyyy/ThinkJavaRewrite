using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Attribute to show a dropdown of achievement IDs from the login JSON save data.
    /// Usage: [AchievementId] public BBParameter&lt;string&gt; achievementId;
    /// The field will show a dropdown populated from AchievementManager.GetAllAchievements()
    /// </summary>
    public class AchievementIdAttribute : PropertyAttribute
    {
    }
}

