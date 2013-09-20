using System;
using System.Web.Mvc;

namespace BookWidget.Domain {

	public sealed class RestHttpVerbFilter : ActionFilterAttribute {

		public override void OnActionExecuting(ActionExecutingContext filterContext) {

			var httpMethod = filterContext.HttpContext.Request.HttpMethod;
			filterContext.ActionParameters["httpVerb"] = httpMethod;
			base.OnActionExecuting(filterContext);
		}
	}

}