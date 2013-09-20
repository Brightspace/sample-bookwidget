using System;
using System.Collections.Generic;

using BookWidget.ViewModels;

namespace BookWidget.Models {

	public interface ICourse {

		IEnumerable<BookItem> AssignedBooks();

		ICourse AddBook( string isbn );

		ICourse RemoveBook( string isbn );
	}
}
