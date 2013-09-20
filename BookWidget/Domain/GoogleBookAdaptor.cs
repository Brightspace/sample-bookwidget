using System;

using BookWidget.ViewModels;

namespace BookWidget.Domain {

	public sealed class GoogleBookAdaptor {

		static public BookItem Adapt( GoogleBook.Item book ) {

			if( book.VolumeInfo.Title == null ) {
				throw new ArgumentNullException();
			}

			var item = new BookItem {Title = book.VolumeInfo.Title, Author = ""};

			if( book.VolumeInfo.Authors != null ) {

				item.Author = string.Join( ", ", book.VolumeInfo.Authors );
			}

			item.Isbn = "";

			if( book.VolumeInfo.IndustryIdentifiers != null ) {

				foreach ( GoogleBook.Item._VolumeInfo.IndustryIdentifierItem id 
					in book.VolumeInfo.IndustryIdentifiers ) {
					if( id.Type.StartsWith( "ISBN_10" ) ) {
						item.Isbn = id.Identifier;
					}
				}
			}

			if( item.Isbn.Length == 0 ) {
				throw new ArgumentNullException();
			}

			item.ThumbnailUrl = "";
			if( ( book.VolumeInfo.ImageLinks != null ) && 
				( book.VolumeInfo.ImageLinks.Thumbnail != null ) ) {
				item.ThumbnailUrl = book.VolumeInfo.ImageLinks.Thumbnail;
			}

			return item;
		}
	}
}