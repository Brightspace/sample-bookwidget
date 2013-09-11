using System;

namespace GoogleBook {
    /// <summary>
    /// Summary description for GoogleBookItem
    /// </summary>
    [Serializable]
    public class Item {
        public string Kind { get; set; }
        public string Id { get; set; } 

        public class _VolumeInfo {
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public string[] Authors { get; set; }
            public string Publisher { get; set; }
            public string PublishedDate { get; set; }
            public string Description { get; set; }

            public class IndustryIdentifierItem {
                public string Type { get; set; }
                public string Identifier { get; set; }
            }

            public IndustryIdentifierItem[] IndustryIdentifiers;

            public string PageCount { get; set; }

            public class _ImageLinks {
                public string SmallThumbnail { get; set; }
                public string Thumbnail { get; set; }
            }

            public _ImageLinks ImageLinks { get; set; }

            public string Language { get; set; }
            public string PreviewLink { get; set; }
            public string InfoLink { get; set; }
            public string CanonicalVolumeLink { get; set; }
        }

        public _VolumeInfo VolumeInfo { get; set; }
    }
}