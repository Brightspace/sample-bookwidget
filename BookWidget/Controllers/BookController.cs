using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using BookWidget.Models;
using BookWidget.ViewModels;

using D2L.Extensibility.AuthSdk;

using Valence;

namespace BookWidget.Controllers
{
    public class RestHttpVerbFilter : ActionFilterAttribute {

        public override void OnActionExecuting(ActionExecutingContext filterContext) {

            var httpMethod = filterContext.HttpContext.Request.HttpMethod;
            filterContext.ActionParameters["httpVerb"] = httpMethod;
            base.OnActionExecuting(filterContext);
        }
    }

    public class BookController : Controller {

        private const string _defaultSessionKey = "sample_app_parameters";

	    private string _defaultAppID = "";
	    private string _defaultAppKey = "";

        private Models.Course Course { get; set; } 

        public BookController( ) {
            Course = new Course();

			_defaultAppID = System.Configuration.ConfigurationManager.AppSettings["ValenceAppId"];
			_defaultAppKey = System.Configuration.ConfigurationManager.AppSettings["ValenceAppKey"];
        }

        //
        // GET: /Book/
        [HttpGet]
        public ActionResult Index() {

            Valence.Request.Parameters param = null;
	        try {
		        param = (Valence.Request.Parameters)Session[ _defaultSessionKey ];
	        } catch( InvalidCastException ) {}

	        if( param == null ) {

		        ViewBag.ErrorMessage = "Unable to retrieve required session parameters.";
				return View( "BookError" );
		        
	        }

	        if ( param.LtiHost == null ) {
	                ViewBag.ErrorMessage = "LTI parameters are not valid.";
                    return View( "BookError" );
			}

			// retrieve the required version information from the LMS
			var factory = new D2LAppContextFactory();
			var appContext = factory.Create( _defaultAppID, _defaultAppKey );
			var hostInfo = new HostSpec( param.Scheme, param.LtiHost, param.LtiPort );

			ID2LUserContext context = appContext.CreateUserContext( Request.Url, hostInfo ) ??
									  appContext.CreateAnonymousUserContext( hostInfo );

			param.UserContext = context;

			var uri = context.CreateAuthenticatedUri( "/d2l/api/versions/", "GET" );
			var request = ( HttpWebRequest )WebRequest.Create( uri );
			request.Method = "GET";

			var versions = new Dictionary<string, string>();

			var handler = new Valence.Request.ErrorHandler();

			Valence.Request.Perform( request, context,
									 delegate( string data ) {
										 var serializer = new JavaScriptSerializer();
										 var requestData =
											 serializer.Deserialize<Valence.ProductVersions[]>( data );

										 foreach( Valence.ProductVersions v in requestData ) {
											 versions[ v.ProductCode ] = v.LatestVersion;
										 }
									 },
									 handler.Process );

			if( handler.IsError ) {

				ViewBag.ErrorMessage = handler.Message;
				return View( "BookError" );
			}
			param.Versions = versions;

            return RedirectToAction( "Assigned" );
        }

        // extract possible LTI launch request parameters to initialize the model
        // consider changing the parameter to a model class that defines what parameters are expected
        // from an LTI launch request.
        [HttpPost]
        public ActionResult Index( FormCollection collection ) {
            // start new user session and populate with auth info
            var parameters = new Valence.Request.Parameters {ClassOrgId = collection["context_id"]};

	        // is not LTI request
            if( parameters.ClassOrgId == null )  {

	            ViewBag.ErrorMessage = "Invalid class org ID.";
                return View( "BookError" );
            }

	        parameters.CanEdit = false;

	        var roles = collection["roles"];
			
			if( roles != null ) {

				var splitRoles = roles.ToString().Split( ',' );

				foreach( var s in splitRoles ) {
					
					if( s.ToLowerInvariant() == "instructor" ) {

						parameters.CanEdit = true;
						break;
					}
				}

			}

            try {

                Uri requestUrl = new Uri(collection["launch_presentation_return_url"]);
                parameters.LtiHost = requestUrl.Host;
				parameters.LtiPort = requestUrl.Port;
                parameters.Scheme = requestUrl.Scheme;
            }
            catch( ArgumentNullException ) {

	            ViewBag.ErrorMessage = "Invalid request URL.";
				return View( "BookError" );
            }

            Session[ _defaultSessionKey ] = parameters;

            var factory = new D2LAppContextFactory();
            var appContext = factory.Create( _defaultAppID, _defaultAppKey );

            var resultUri = new UriBuilder( Request.Url.Scheme,
                                            Request.Url.Host,
                                            Request.Url.Port,
                                            Request.Url.AbsolutePath ).Uri;

            var host = new HostSpec( parameters.Scheme, parameters.LtiHost, parameters.LtiPort );
            var uri = appContext.CreateUrlForAuthentication( host, resultUri );

            return Redirect( uri.ToString() );
        }
        
        [HttpGet]
        public ActionResult Error( string message ) {

            ViewBag.ErrorMessage = message;
            return View( "BookError" );
        }

        [RestHttpVerbFilter]
        public ActionResult Assigned( string isbn, string httpVerb ) {

            var sessionData = Session[_defaultSessionKey];
            if( sessionData == null ) {
                return View( "BookError" );
            }
            var parameters = ( Valence.Request.Parameters )sessionData;

            string errorMessage = "";

            switch (httpVerb) {
                case "GET":
					BookItem[] items = Course.AssignedBooks( parameters, ref errorMessage );

					ActionResult resultView;

					if( errorMessage.Length > 0 ) {

						resultView = View( "BookError", errorMessage );
					} else {

						var result = new BookItemResults { Items = items, CanEdit = parameters.CanEdit };
						resultView = View( "BookList", result );
					}
		            return resultView;
                case "POST":
                    if( !Course.AddBook( parameters, isbn, ref errorMessage ) ) {
                        return Json( new {Error = true, Message = "Unable to assign book(" + isbn + ") to course." + errorMessage} );
                    }
		            return Json( new {Error = false} );
                case "DELETE":
                    if( !Course.RemoveBook( parameters, isbn, ref errorMessage ) ) {
                        return Json( new {Error = true, Message = "Unable to remove book(" + isbn + ") to course." + errorMessage} );
                    }
		            return Json( new {Error = false} );
            }

            return View( "BookError" );
        }
    }
}
