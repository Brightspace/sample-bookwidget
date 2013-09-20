using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using BookWidget.Domain;
using BookWidget.Models;
using BookWidget.ViewModels;

using D2L.Extensibility.AuthSdk;

namespace BookWidget.Controllers
{
	public class BookController : Controller {

		private const string SESSION_KEY = "bookwidget_sample_parameters";

		private readonly string m_defaultAppId = "";
		private readonly string m_defaultAppKey = "";

		public BookController( ) {

			m_defaultAppId = System.Configuration.ConfigurationManager.AppSettings["ValenceAppId"];
			m_defaultAppKey = System.Configuration.ConfigurationManager.AppSettings["ValenceAppKey"];
		}

		private bool CanEdit( string roles ) {

			if( !string.IsNullOrEmpty( roles ) ) {

				var splitRoles = roles.Split( ',' );

				return splitRoles.Any( s => s.ToLowerInvariant() == "instructor" );
			}

			return false;
		}

		private Uri GenerateAuthRedirect( Uri returnUri, Uri requestUri ) {

			if(( requestUri == null ) || ( returnUri == null )) {
				throw new ArgumentNullException();
			}

			var factory = new D2LAppContextFactory();
			var appContext = factory.Create( m_defaultAppId, m_defaultAppKey );

			var resultUri = new UriBuilder( requestUri.Scheme,
											requestUri.Host,
											requestUri.Port,
											requestUri.AbsolutePath ).Uri;

			var host = new HostSpec( returnUri.Scheme, returnUri.Host, returnUri.Port );
			var redirectUri = appContext.CreateUrlForAuthentication( host, resultUri );

			return redirectUri;
		}

		// GET: /Book/
		[HttpGet]
		public ActionResult Index() {

			string oauthKey = System.Configuration.ConfigurationManager.AppSettings["OauthKey"];
			string oauthSecret = System.Configuration.ConfigurationManager.AppSettings["OauthSecret"];

			var param = Session[SESSION_KEY] as SessionParameters;

			if( param == null ) {

				ViewBag.ErrorMessage = "Unable to retrieve required session param.";
				return View( "BookError" );
				
			}

			if ( param.LtiUri == null ) {

					ViewBag.ErrorMessage = "LTI param are not valid.";
					return View( "BookError" );
			}

			// retrieve the required version information from the LMS
			var factory = new D2LAppContextFactory();
			var appContext = factory.Create( m_defaultAppId, m_defaultAppKey );
			var hostInfo = new HostSpec( param.LtiUri.Scheme, param.LtiUri.Host, param.LtiUri.Port );

			ID2LUserContext context = appContext.CreateUserContext( Request.Url, hostInfo );

			if( context == null ) {

				ViewBag.ErrorMessage = "Unable to create user context.";
				return View( "BookError" );
			}

			param.UserContext = context;

			return RedirectToAction( "Assigned" );
		}

		[HttpPost]
		public ActionResult Index( FormCollection collection ) {

			// verify OAuth
			var parameters = new SessionParameters { ClassOrgId = collection["context_id"]   };

			if( parameters.ClassOrgId == null )  {

				ViewBag.ErrorMessage = "Invalid class org ID.";
				return View( "BookError" );
			}

			parameters.CanEdit = CanEdit( collection["roles"] );

			Uri redirectUri;

			try {

				Uri requestUrl = new Uri( collection["lis_outcome_service_url"] );
				parameters.LtiUri = new UriBuilder(
					requestUrl.Scheme,
					requestUrl.Host,
					requestUrl.Port
				).Uri;

				Session[ SESSION_KEY ] = parameters;

				redirectUri = GenerateAuthRedirect( parameters.LtiUri, Request.Url );

			} catch( ArgumentNullException e ) {

				ViewBag.ErrorMessage = "Invalid request URL. " + e.Message;
				return View( "BookError" );
			}

			return Redirect( redirectUri.ToString( )); 
		}
		
		[HttpGet]
		public ActionResult Error( string message ) {

			ViewBag.ErrorMessage = message;
			return View( "BookError" );
		}

		[RestHttpVerbFilter]
		public ActionResult Assigned( string isbn, string httpVerb ) {

			var param = Session[SESSION_KEY] as SessionParameters;
			if( param == null ) {
				return View( "BookError" );
			}

			ICourse course = new Course( param );

			string errorMessage = string.Empty;

			try {
				switch( httpVerb ) {
					case "GET":
						var items = course.AssignedBooks();
						var results = new BookItemResults {Items = items, CanEdit = param.CanEdit};
						return View( "BookList", results );

					case "POST":
						errorMessage = "Unable to assign book(" + isbn +") to course.";
						course.AddBook( isbn );
						return Json( new {Error = false} );

					case "DELETE":
						errorMessage = "Unable to remove book(" + isbn +") from course.";
						course.RemoveBook( isbn );
						return Json( new {Error = false} );
				}
			} catch( InvalidOperationException e ) {

				if( httpVerb == "GET" ) {
					ViewBag.ErrorMessage = e.Message;
					return View( "BookError" );
				}

				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return Json( new { Error = true, Message = errorMessage + e.Message } );
			}

			return View( "BookError" );
		}
	}
}
