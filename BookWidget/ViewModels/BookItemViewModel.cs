using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookWidget.ViewModels
{
    public class BookItem {

        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class BookItemResults {

        public BookItem[] Items { get; set; }

		public bool CanEdit { get; set; }
    }

}