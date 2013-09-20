using System;
using System.Collections.Generic;

namespace Valence {

	public class Book {

		public string OrgUnitId { get; set; }
		public string Isbn { get; set; }
	}

	public class IsbnAssociation {

		public string Isbn { get; set; }
	}

	public class ProductVersions {

		public string LatestVersion { get; set; }
		public string ProductCode { get; set; }
		public List<string> SupportedVersions { get; set; }
	}
}