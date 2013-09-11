using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

using BookWidget.ViewModels;

using D2L.Extensibility.AuthSdk;

namespace BookWidget.Models
{
    public class Course {

        private BookItem FromGoogleBookToBookItem( GoogleBook.Item book ) {

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

		private List<BookItem> GetBooksForClass( 
				Valence.Request.Parameters param, 
				Valence.OrgUnitInfo course, 
				ref string message ) {

			Uri uri = param.UserContext.CreateAuthenticatedUri( 
				"/d2l/api/"+ param.ApiVersion("le") + "/" + course.Id + "/content/isbn/", "GET" );

			var request = (HttpWebRequest)WebRequest.Create( uri );
			request.Method = "GET";

			var handler = new Valence.Request.ErrorHandler();
			var result = new List<BookItem>();

			Valence.Request.Perform( 
				request, param.UserContext,
				 delegate( string data ) {

					 var serializer = new JavaScriptSerializer();
					 var books = serializer.Deserialize<Valence.Book[]>( data );

					 if( ( books == null ) || ( books.Length <= 0 ) ) {
						 return;
					 }

					 foreach( Valence.Book b in books ) {

						 try {
							 GoogleBook.Volume volume = GoogleBook.Api.Query( b.Isbn );

							 if( volume.TotalItems != 0 ) {

								 GoogleBook.Item item = volume.items.First();

								 BookItem book = FromGoogleBookToBookItem( item );

								 result.Add( book );
							 }
						 } catch( ArgumentNullException ) {}
					 }
				 },
				 handler.Process );

			if( handler.IsError ) {
				message = "Unable to retrieve assigned books. " + handler.Message;
			}

			return result;
		}

        public BookItem[] AssignedBooks( Valence.Request.Parameters param, ref string message ) {

	        var classOrgInfo = new Valence.OrgUnitInfo() {Id = param.ClassOrgId};

			var result = new List<BookItem>();

			result.AddRange( GetBooksForClass( param, classOrgInfo, ref message ) ); 

			return result.ToArray();
        }

        public bool AddBook( Valence.Request.Parameters param, string isbn, ref string message ) {

            Uri uri = param.UserContext.CreateAuthenticatedUri(
                "/d2l/api/" + param.ApiVersion( "le" ) + "/" + param.ClassOrgId + "/content/isbn/", "POST" );

            var bookIsbn = new Valence.IsbnAssociation { Isbn = isbn };
            var serializer = new JavaScriptSerializer();
            String postData = serializer.Serialize(bookIsbn);

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json";

            using( var requestWriter = new StreamWriter( request.GetRequestStream() ) ) {
                requestWriter.Write( postData );
            }

            bool result = true;

            var handler = new Valence.Request.ErrorHandler();

            Valence.Request.Perform( request, param.UserContext,
                                     delegate( string data ) {
                                         var responseSerializer = new JavaScriptSerializer();
                                         var bookResult = responseSerializer.Deserialize<Valence.Book>( data );
                                         if (bookResult.Isbn != isbn) {
                                             result = false;
                                         }
                                     },
                                     handler.Process);

            if( handler.IsError ) {

                result = false;
                message = "Unable to add books to course. " + handler.Message;
            }
            return result;
        }

        public bool RemoveBook( Valence.Request.Parameters param, string isbn, ref string message ) {

            var uri = param.UserContext.CreateAuthenticatedUri(
                "/d2l/api/" + param.ApiVersion( "le" ) + "/" + param.ClassOrgId + "/content/isbn/" + isbn, "DELETE" );

            var request = (HttpWebRequest) WebRequest.Create( uri );
            request.Method = "DELETE";

            var handler = new Valence.Request.ErrorHandler();

            Valence.Request.Perform( request, param.UserContext, delegate( string data ) { }, handler.Process );

            if( handler.IsError ) {
                message = "Unable to remove books from course. " + handler.Message;
            }

            return !handler.IsError;
        }
    }
}
