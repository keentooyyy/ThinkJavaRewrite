using UnityEngine;
using System.Collections.Generic;

namespace GameUI
{
    /// <summary>
    /// ScriptableObject for loading screen content (quotes and images)
    /// Create instances via: Right-click → Create → Game → Loading Screen Content
    /// </summary>
    [CreateAssetMenu(fileName = "LoadingScreenContent", menuName = "Game/Loading Screen Content")]
    public class LoadingScreenContent : ScriptableObject
    {
        [System.Serializable]
        public class Quote
        {
            [Tooltip("The motivational quote text")]
            [TextArea(2, 4)]
            public string quoteText;

            [Tooltip("Name of the person who said it (e.g., 'Mark Zuckerberg')")]
            public string authorName;

            [Tooltip("Optional: Title/role of the person (e.g., 'CEO of Meta')")]
            public string authorTitle;
        }

        [Header("Quotes")]
        [Tooltip("List of motivational programming quotes to display during loading")]
        public List<Quote> quotes = new List<Quote>();

        [Header("Images/Banners")]
        [Tooltip("List of sprites/images to display as banners during loading")]
        public List<Sprite> bannerImages = new List<Sprite>();

        /// <summary>
        /// Get a random quote from the list
        /// </summary>
        public Quote GetRandomQuote()
        {
            if (quotes == null || quotes.Count == 0)
                return null;

            return quotes[Random.Range(0, quotes.Count)];
        }

        /// <summary>
        /// Get a random banner image from the list
        /// </summary>
        public Sprite GetRandomBanner()
        {
            if (bannerImages == null || bannerImages.Count == 0)
                return null;

            return bannerImages[Random.Range(0, bannerImages.Count)];
        }
    }
}

