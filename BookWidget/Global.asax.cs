using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BookWidget {

	public class MvcApplication : System.Web.HttpApplication {

		public static void RegisterGlobalFilters( GlobalFilterCollection filters ) {
			filters.Add( new HandleErrorAttribute() );
		}

		public static void RegisterRoutes(RouteCollection routes) {

			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			routes.MapRoute(
				"BookRoute",
				"Book/{action}/{isbn}",
				new { controller = "Book", action = "Index", isbn = UrlParameter.Optional }
			);
		}

		protected void Application_Start() {

			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters( GlobalFilters.Filters );
			RegisterRoutes( RouteTable.Routes );
		}
	}
}