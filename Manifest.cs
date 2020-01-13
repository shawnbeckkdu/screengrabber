using System.Collections.Generic;

namespace ScreenGrab
{
    [System.Serializable]
    public struct Manifest
    {
        public List<string> filenames;
        
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public Manifest(List<string> initFilenames)
        {
            this.filenames = initFilenames;
        }
    }
}