using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using BookWidget.Domain;
using BookWidget.ViewModels;

using D2L.Extensibility.AuthSdk.Restsharp;

using RestSharp;

using Valence;

namespace BookWidget.Models {

	public class Course : ICourse {

		private const string BASE_CONTENT_ROUTE = "/d2l/api/{component}/{version}/{orgId}/content/isbn/";

		private readonly SessionParameters m_parameters;

		public Course( SessionParameters parameters ) {
			m_parameters = parameters;
		}

		private string ApiVersion( string name ) {

			const string ERROR_MESSAGE = "Unable to retrieve api version for '";

			if( m_parameters.Versions == null ) {
				m_parameters.Versions = RequestApiVersions();
			}

			try {
				return m_parameters.Versions[name];
			} catch( ArgumentNullException e ) {
				throw new InvalidOperationException( ERROR_MESSAGE + name + "'", e);
			} catch( KeyNotFoundException e ) {
				throw new InvalidOperationException( ERROR_MESSAGE + name + "'", e );
			}
		}

		private Dictionary<string,string>  RequestApiVersions() {

			const string ROUTE = "/d2l/api/versions/";

			var client = CreateClient();

			var request = new RestRequest( ROUTE, Method.GET );

			var response = client.Execute<List<Valence.ProductVersions>>( request );

			if( response.StatusCode != HttpStatusCode.OK ) {

				string message = 
					"Unable to retrieve learning environment API versions. Status returned: " +
					response.StatusCode;

				throw new InvalidOperationException( message );
			}

			var versions = new Dictionary<string, string>();

			foreach( var version in response.Data ) {
				versions[version.ProductCode] = version.LatestVersion;
			}

			return versions;
		}

		private void AddBaseContentUrlSegments( RestRequest request ) {

			request.AddUrlSegment( "component", "le" );
			request.AddUrlSegment( "version", ApiVersion( "le" ) );
			request.AddUrlSegment( "orgId", m_parameters.ClassOrgId );
		}

		RestClient CreateClient( bool hasAuth = true ) {

			var client = new RestClient( m_parameters.LtiUri.ToString() );

			if( hasAuth ) {
				client.Authenticator = new ValenceAuthenticator( m_parameters.UserContext );
			}

			return client;
		}

		RestRequest CreateContentRequest( string route, RestSharp.Method method ) {

			var fullRoute = BASE_CONTENT_ROUTE;

			if( !string.IsNullOrEmpty( route )) {
				fullRoute += route;
			}

			var request = new RestRequest( fullRoute, method );

			AddBaseContentUrlSegments( request );

			request.RequestFormat = DataFormat.Json;

			return request;
		}

		public IEnumerable<BookItem> AssignedBooks() {

			const string ROUTE = "";

			var client = CreateClient();

			var request = CreateContentRequest( ROUTE, Method.GET) ;

			var response = client.Execute<List<Valence.Book>>( request );

			if( response.StatusCode != HttpStatusCode.OK ) {
				throw new InvalidOperationException( "Unable to get isbns from course. Returned status: " + response.StatusCode );
			}

			var bookList = new List<BookItem>();

			foreach( Valence.Book b in response.Data ) {

				try {
					GoogleBook.Volume volume = GoogleBook.Api.Query( b.Isbn );

					if( volume.TotalItems != 0 ) {

						GoogleBook.Item item = volume.items.First();

						BookItem book = GoogleBookAdaptor.Adapt( item );

						// ensure that the isbn in the LMS is set.  Sometimes GoogleBooks returns an alternate ISBN
						book.Isbn = b.Isbn; 

						bookList.Add( book );
					}
				} catch( ArgumentNullException ) {}
			}

			return bookList;
		}

		public ICourse AddBook( string isbn ) {

			const string ROUTE = "";

			var bookIsbn = new Valence.IsbnAssociation { Isbn = isbn };

			var client = CreateClient();

			var request = CreateContentRequest( ROUTE, Method.POST );

			request.AddBody( bookIsbn );

			var response = client.Execute<Valence.Book>( request );

			if(( response.StatusCode != HttpStatusCode.OK ) || ( response.Data.Isbn != isbn )) {
				throw new InvalidOperationException( "Unable to add isbn to course. Returned status: " + response.StatusCode );
			}

			return this;
		}

		public ICourse RemoveBook( string isbn ) {

			const string ROUTE = "{isbn}";

			var client = CreateClient();

			var request = CreateContentRequest( ROUTE, Method.DELETE );

			request.AddUrlSegment( "isbn", isbn );

			var response = client.Execute( request );

			if( response.StatusCode != HttpStatusCode.OK ) {
				throw new InvalidOperationException( "Unable to delete isbn from course. Returned status: " + response.StatusCode );
			}

			return this;
		}
	}
}
