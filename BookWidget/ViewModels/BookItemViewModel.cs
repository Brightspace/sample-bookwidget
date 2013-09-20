using System;
using System.Collections.Generic;

namespace BookWidget.ViewModels
{
	public class BookItem {

		public string Title { get; set; }
		public string Author { get; set; }
		public string Isbn { get; set; }
		public string ThumbnailUrl { get; set; }
	}

	public class BookItemResults {

		public IEnumerable<BookItem> Items { get; set; }

		public bool CanEdit { get; set; }
	}

}