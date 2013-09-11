using System;

namespace Valence {

    /// <summary>
    /// Summary description for PagedResultSet
    /// </summary>
    /// 
    [Serializable]
    public struct PagedResultSet<T> {

		public struct _PagingInfo {
			public string Bookmark;
			public bool HasMoreItems;
		}

	    public _PagingInfo PagingInfo;

	    public T[] Items;
    }

    /// <summary>
    /// Summary description for Book
    /// </summary>
    [Serializable]
    public struct Book {

        public string OrgUnitId;
        public string Isbn;
    }

    /// <summary>
    /// Summary description for IsbnAssociation
    /// </summary>
    [Serializable]
    public struct IsbnAssociation {

        public string Isbn;
    }

    /// <summary>
    /// Summary description for ProductVerions
    /// </summary>
    /// 
    [Serializable]
    public struct ProductVersions {

        public string LatestVersion;
        public string ProductCode;
        public string[] SupportedVersions;
    }

    /// <summary>
    /// Summary description for BasicOrgUnit
    /// </summary>
    /// 
    [Serializable]
    public struct BasicOrgUnit {

	    public string Identifier;
	    public string Name;
	    public string Code;
    }

    /// <summary>
    /// Summary description for OrgUnitTypeInfo
    /// </summary>
    /// 
	public struct OrgUnitTypeInfo {

	    public string Id;
	    public string Code;
	    public string Name;
    }

    /// <summary>
    /// Summary description for OrgUnitInfo
    /// </summary>
    /// 
	public struct OrgUnitInfo {

	    public string Id;
	    public OrgUnitTypeInfo Type;
	    public string Name;
	    public string Code;
    }

    /// <summary>
    /// Summary description for AccessInfo
    /// </summary>
    /// 
	public struct AccessInfo {

		public bool IsActive;
		public string StartDate;
		public string EndDate;
		public bool CanAccess;
	}


    /// <summary>
    /// Summary description for MyOrgUnitInfo
    /// </summary>
    /// 
    [Serializable]
    public struct MyOrgUnitInfo {

	    public OrgUnitInfo OrgUnit;

	    public AccessInfo Access;
    }

}