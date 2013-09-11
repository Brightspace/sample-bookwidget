using System;

namespace GoogleBook {
    /// <summary>
    /// Summary description for GoogleBookVolume
    /// </summary>
    [Serializable]
    public class Volume {
        public String Kind { get; set; }
        public int TotalItems { get; set; }
        public Item[] items { get; set; }
    }
}