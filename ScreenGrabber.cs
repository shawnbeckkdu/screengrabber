#pragma warning disable CS0414
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScreenGrab
{

    /// <summary>
    /// ScreenGrabber
    /// The ScreenGrabber requires MonoBehaviour.Update() to allow shortcut key screenshots
    /// Use ScreenGrabber.Capture() to take a screenshot
    /// Use ScreenGrabber.Load(string) to load a screenshot
    /// Use ScreenGrabber.Delete(string) to delete a screenshot
    /// </summary>
    public class ScreenGrabber : MonoBehaviour
    {
        [Header("Production Settings")]
        [SerializeField] private bool allowProductionScreenGrab = false;

        [Header("Editor Settings")]
        [SerializeField]private EditorLocation editorLocation = EditorLocation.StreamingAssetsFolder;
        private enum EditorLocation {Desktop,StreamingAssetsFolder}  
        
        public static Manifest ManifestData { get; private set;}
        private string manifestPath;


        #region Singleton
        public static ScreenGrabber Instance { get; set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        #endregion




        #region Initialization
        
        /// <summary>
        /// Setup
        /// </summary>
        private void OnEnable()
        {
            manifestPath = Path.Combine(Application.persistentDataPath, "ScreenGrabData");
            ManifestData = LoadManifest();
        }

        #endregion





        #region Load & Save


        /// <summary>
        /// Loads the manifest.
        /// </summary>
        private Manifest LoadManifest()
        {
            Manifest m = new Manifest( new List<string>() );

            if (File.Exists(manifestPath))
            {
                string json = File.ReadAllText(manifestPath);
                m = JsonUtility.FromJson<ScreenGrab.Manifest>(json);
            }

            return m;
        }


        /// <summary>
        /// Adds an entry to the manifest
        /// </summary>
        private void AddManifestEntry(string entry)
        {
            ManifestData.filenames.Add(entry);
            SaveManifest();
        }


        /// <summary>
        /// Removes a manifest entry.
        /// </summary>
        private void RemoveManifestEntry(string entry)
        { 
            ManifestData.filenames.Remove(entry);
            SaveManifest();
        }
        
        
        /// <summary>
        /// Removes a manifest entry.
        /// </summary>
        private void RemoveManifestEntry(int entry)
        { 
            ManifestData.filenames.RemoveAt(entry);
            SaveManifest();
        }

        
        
        /// <summary>
        /// Saves the manifest.
        /// </summary>
        private void SaveManifest()
        { 
            string json = JsonUtility.ToJson(ManifestData);
            File.WriteAllText(manifestPath, json);
        }



        #endregion




        #region Capture


        /// <summary>
        /// Captures a screenshot and return a Texture2D
        /// Production Path: Application.persistentDataPath
        /// Editor Path: Desktop
        /// Returns the image filename for later retrieval
        /// </summary>
        /// <returns>The capture.</returns>
        public static string Capture()
        {

            // Define filename
            string fileName = NewFilename;

            // Define Path
            string path = Path.Combine(ScreenshotDirectory, fileName);

            // Do Screenshot
            ScreenCapture.CaptureScreenshot(path); // Do screenshot
            
            // Save to manifest
            if (Instance.editorLocation != EditorLocation.Desktop)
            {
                Instance.AddManifestEntry(fileName);
            }
            
            // Alert Editor
            #if UNITY_EDITOR
            BeepEditor(path);
            #endif

            return fileName;

        }
        
        
        
        
        /// <summary>
        /// Gets a specific screenshot.
        /// </summary>
        public static Texture2D Load(string fileName)
        {
            string screenShotPath = Path.Combine(ScreenshotDirectory, fileName); // Get screenshot path
            Texture2D tex = null; // Texture to return
        
            // Load image if exists
            if (File.Exists(screenShotPath))
            {

                // Read Image data from byte[]
                Byte[] data = File.ReadAllBytes(screenShotPath);
                
                // Create a texture. Texture size does not matter, since
                // LoadImage will replace with with incoming image size.
                tex = new Texture2D(2, 2);
                tex.LoadImage(data); // Use Image Conversion to load byte data as texture2D

            }
            
            return tex;
        }
        
        
        
        /// <summary>
        /// Delete the specified fileName and returns a true/false deletion result
        /// </summary>
        public static bool Delete(string fileName )
        { 
            // Check Manifest before deleting; 
            // Return false if requested file doesn't exist in manifest
            if ( ManifestData.filenames.Contains(fileName) == false ) return false;
            
            // Check if file exists and delete it
            string path = Path.Combine( ScreenshotDirectory, fileName );
            
            if ( File.Exists( path ) )
            { 
                File.Delete( path );
                return true;    // File was deleted
            }
            else
            { 
                return false;   // No File was found
            }
        }
        
        


        
        /// <summary>
        /// Generates a new filename based on editor/production settings
        /// </summary>
        /// <value>The new filename.</value>
        private static string NewFilename
        {
            get 
            {
                string newFileName = string.Empty;

                if (Application.isEditor && Instance.editorLocation == EditorLocation.Desktop )
                {
                    newFileName = $"{Application.productName} {DateTime.Now.ToString("hh-mm-ss tt")}.png";
                }
                else
                {
                    newFileName = $"IMG{ManifestData.filenames.Count}.png";
                }

                return newFileName;
            }
        }



        /// <summary>
        /// Gets the screenshot directory based on editor/production settings
        /// </summary>
        private static string ScreenshotDirectory
        {
            get 
            {
                string dir = string.Empty;
                if (Application.isEditor)
                {
                    switch (Instance.editorLocation)
                    {
                        case EditorLocation.Desktop:
                            dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                            break;
                        case EditorLocation.StreamingAssetsFolder:
                            dir = Application.streamingAssetsPath;
                            break;
                    }

                }
                else
                {
                    dir = Application.persistentDataPath;
                }

                return dir;
            }
        }
        
        #endregion





        #if UNITY_EDITOR        
        #region Editor


        /// <summary>
        /// Editor Screen Grab
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
            {

                // Prevent production screen grab if not allowed
                if (Application.isEditor && allowProductionScreenGrab == false) return;

                // Do Screen Capture
                Capture();

            }
        }



        /// <summary>
        /// Beeps the editor with path print out
        /// </summary>
        private static void BeepEditor(string path)
        {
            EditorApplication.Beep();
            Debug.Log($"<color=cyan>Screenshot saved to {path}</color>");
        }



        #endregion
        #endif

    }
}