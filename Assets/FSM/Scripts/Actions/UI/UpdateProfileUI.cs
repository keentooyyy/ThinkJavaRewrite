using System.Collections;
using System.Security.Cryptography;
using System.Text;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using GameProgress;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/UI")]
    [Description("Update Profile UI with student data from ES3. Parses and displays student information when profile button is tapped.")]
    public class UpdateProfileUI : ActionTask
    {
        [UnityEngine.Header("UI Text Fields")]
        [Tooltip("Text field to display student ID")]
        public BBParameter<TMP_Text> studentIdText;
        
        [Tooltip("Text field to display student name (first + middle initial + last + suffix)")]
        public BBParameter<TMP_Text> studentNameText;
        
        [Tooltip("Text field to display section (e.g., CS1A)")]
        public BBParameter<TMP_Text> sectionText;
        
        [Tooltip("Text field to display bio")]
        public BBParameter<TMP_Text> bioText;

        [UnityEngine.Header("Profile Picture")]
        [Tooltip("Image component to display profile picture")]
        public BBParameter<Image> profileImage;

        private Coroutine loadImageCoroutine;
        private const string PROFILE_PIC_CACHE_PREFIX = "ProfilePicCache_";

        protected override string info
        {
            get { return "Update Profile UI"; }
        }

        protected override void OnExecute()
        {
            // Get complete login response from ES3 (includes status and all fields)
            var loginResponse = LoginManager.GetLoginResponse();
            
            // Fallback to student data if full response not available
            var studentData = loginResponse?.student ?? LoginManager.GetStudentData();
            
            if (studentData == null)
            {
                // No student data available - show placeholder or empty
                if (studentIdText.value != null)
                    studentIdText.value.text = "Not logged in";
                if (studentNameText.value != null)
                    studentNameText.value.text = "";
                if (sectionText.value != null)
                    sectionText.value.text = "";
                if (bioText.value != null)
                    bioText.value.text = "";
                if (profileImage.value != null)
                    profileImage.value.sprite = null;
                
                Debug.LogWarning("UpdateProfileUI: No student data found in ES3");
                EndAction(true);
                return;
            }

            // Update student ID
            if (studentIdText.value != null)
            {
                studentIdText.value.text = !string.IsNullOrEmpty(studentData.student_id) 
                    ? studentData.student_id 
                    : "N/A";
            }

            // Update student name (first + middle initial + last + suffix)
            if (studentNameText.value != null)
            {
                string fullName = FormatFullName(loginResponse);
                studentNameText.value.text = !string.IsNullOrEmpty(fullName) ? fullName : "N/A";
            }

            // Update section (full_section)
            if (sectionText.value != null)
            {
                string section = loginResponse?.section?.full_section ?? "";
                sectionText.value.text = !string.IsNullOrEmpty(section) ? section : "N/A";
            }

            // Update bio
            if (bioText.value != null)
            {
                string bio = loginResponse?.profile?.bio ?? "";
                bioText.value.text = !string.IsNullOrEmpty(bio) ? bio : "Bio is unset";
            }

            // Load profile picture from URL
            bool loadingImage = false;
            if (profileImage.value != null && loginResponse != null && loginResponse.profile != null)
            {
                string profilePictureUrl = loginResponse.profile.profile_picture;
                if (!string.IsNullOrEmpty(profilePictureUrl))
                {
                    loadingImage = true;
                    loadImageCoroutine = CoroutineHelper.StartStaticCoroutine(LoadProfilePicture(profilePictureUrl));
                }
                else
                {
                    // No profile picture URL - clear image
                    Debug.LogWarning("UpdateProfileUI: Profile picture URL is empty or null");
                    profileImage.value.sprite = null;
                    profileImage.value.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning($"UpdateProfileUI: Cannot load profile picture - profileImage: {profileImage.value != null}, loginResponse: {loginResponse != null}, profile: {loginResponse?.profile != null}");
            }

            
            // Only end action immediately if we're not loading an image
            // If loading image, the coroutine will call EndAction when done
            if (!loadingImage)
            {
                EndAction(true);
            }
        }

        /// <summary>
        /// Format full name with middle initial and suffix
        /// Format: "FirstName MI LastName Suffix" if both exist
        ///         "FirstName MI LastName" if only middle_initial exists
        ///         "FirstName LastName Suffix" if only suffix exists
        ///         "FirstName LastName" if neither exists
        /// </summary>
        private string FormatFullName(GameSaveAPIManager.LoginResponse loginResponse)
        {
            if (loginResponse?.student == null)
                return "";

            string firstName = loginResponse.student.first_name ?? "";
            string lastName = loginResponse.student.last_name ?? "";
            string middleInitial = loginResponse.profile?.middle_initial ?? "";
            string suffix = loginResponse.profile?.suffix ?? "";

            // Trim all values
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            middleInitial = middleInitial.Trim();
            suffix = suffix.Trim();

            // Build name parts
            System.Text.StringBuilder nameBuilder = new System.Text.StringBuilder();

            // First name
            if (!string.IsNullOrEmpty(firstName))
            {
                nameBuilder.Append(firstName);
            }

            // Middle initial
            if (!string.IsNullOrEmpty(middleInitial))
            {
                if (nameBuilder.Length > 0) nameBuilder.Append(" ");
                nameBuilder.Append(middleInitial);
            }

            // Last name
            if (!string.IsNullOrEmpty(lastName))
            {
                if (nameBuilder.Length > 0) nameBuilder.Append(" ");
                nameBuilder.Append(lastName);
            }

            // Suffix
            if (!string.IsNullOrEmpty(suffix))
            {
                if (nameBuilder.Length > 0) nameBuilder.Append(" ");
                nameBuilder.Append(suffix);
            }

            return nameBuilder.ToString();
        }

        private IEnumerator LoadProfilePicture(string url)
        {
            if (profileImage.value == null || string.IsNullOrEmpty(url))
            {
                EndAction(true);
                yield break;
            }

            // Generate cache key from URL
            string cacheKey = GetCacheKey(url);
            
            // Try to load sprite from cache first
            Sprite cachedSprite = LoadCachedSprite(cacheKey);
            if (cachedSprite != null)
            {
                // Use cached sprite
                profileImage.value.sprite = cachedSprite;
                profileImage.value.enabled = true;
                EndAction(true);
                yield break;
            }

            // Cache miss - download from URL
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    if (texture != null)
                    {
                        // Create sprite from texture
                        Sprite sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f)
                        );
                        
                        // Save sprite to cache
                        SaveSpriteToCache(cacheKey, sprite);
                        
                        // Set sprite to image component
                        profileImage.value.sprite = sprite;
                        profileImage.value.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning($"UpdateProfileUI: Failed to create texture from downloaded data");
                    }
                }
                else
                {
                    Debug.LogWarning($"UpdateProfileUI: Failed to load profile picture from {url}: {www.error} (Result: {www.result})");
                    // Clear image on failure
                    if (profileImage.value != null)
                    {
                        profileImage.value.sprite = null;
                    }
                }
            }
            
            // End action after image loading completes (success or failure)
            EndAction(true);
        }

        /// <summary>
        /// Generate a cache key from URL using MD5 hash
        /// </summary>
        private string GetCacheKey(string url)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return PROFILE_PIC_CACHE_PREFIX + sb.ToString();
            }
        }

        /// <summary>
        /// Load cached sprite from ES3
        /// </summary>
        private Sprite LoadCachedSprite(string cacheKey)
        {
            try
            {
                if (ES3.KeyExists(cacheKey))
                {
                    // Load PNG bytes from ES3
                    byte[] imageBytes = ES3.Load<byte[]>(cacheKey);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        // Create texture from bytes
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (texture.LoadImage(imageBytes))
                        {
                            // Make texture readable
                            texture.Apply(false, false);
                            
                            // Create sprite from texture
                            Sprite sprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f),
                                100f // pixels per unit
                            );
                            
                            return sprite;
                        }
                        else
                        {
                            Debug.LogWarning("UpdateProfileUI: Failed to load image from cached bytes");
                            Object.Destroy(texture);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"UpdateProfileUI: Failed to load cached sprite: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Save sprite to ES3 cache as PNG bytes
        /// </summary>
        private void SaveSpriteToCache(string cacheKey, Sprite sprite)
        {
            try
            {
                if (sprite != null && sprite.texture != null)
                {
                    // Encode sprite's texture to PNG bytes
                    byte[] pngBytes = sprite.texture.EncodeToPNG();
                    if (pngBytes != null && pngBytes.Length > 0)
                    {
                        // Save PNG bytes to ES3
                        ES3.Save(cacheKey, pngBytes);
                    }
                    else
                    {
                        Debug.LogWarning($"UpdateProfileUI: Failed to encode sprite texture to PNG");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"UpdateProfileUI: Failed to save sprite to cache: {e.Message}");
            }
        }

        protected override void OnStop()
        {
            if (loadImageCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(loadImageCoroutine);
                loadImageCoroutine = null;
            }
        }
    }
}

