using System;
using System.Collections.Generic;

using D2L.Extensibility.AuthSdk;

namespace BookWidget.Models {

	public class SessionParameters {

		public string ClassOrgId { get; set; }

		public bool CanEdit { get; set; }

		public ID2LUserContext UserContext { get; set; }

		public Uri LtiUri { get; set; }

		public Dictionary<string, string> Versions { get; set; }
	}
}