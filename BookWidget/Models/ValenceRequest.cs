using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

using D2L.Extensibility.AuthSdk;

namespace Valence {
    public sealed class Request {

        public sealed class Parameters {
            public string ClassOrgId { get; set; }

			public bool CanEdit { get; set; }

            public string LtiHost { get; set; }
            public int LtiPort { get; set; }
            public string Scheme { get; set; }

            public ID2LUserContext UserContext { get; set; }

            public Dictionary<string, string> Versions { get; set; }

            public string ApiVersion( string name ) {
                if (name == null) {
                    return "1.0";
                }
                string result = name + "/1.0";
                try {
                    result = name + "/" + this.Versions[name];
                }
                catch (KeyNotFoundException) {}
                return result;
            }
        }

        public class ErrorHandler {

            public bool IsError { get; set; }
            public string Message { get; set; }

            public ErrorHandler() {

                IsError = false;
                Message = "";
            }

            public void Process( RequestResult result, Uri requestUrl ) {

                Message += result.ToString() + " for " + requestUrl.AbsoluteUri + " : ";
	            IsError = true;

                switch (result) {
                    case RequestResult.INTERNAL_SERVER_ERROR:
                        break;
                    case RequestResult.BAD_REQUEST:
                        Message += "The ISBN provided is invalid.";
                        break;
                    case RequestResult.NOT_FOUND:
                        Message += "association not found for this class and book.";
                        break;
                    case RequestResult.RESULT_INVALID_SIG:
                        break;
                    case RequestResult.RESULT_NO_PERMISSION:
                        Message += "You don't have permission to perform this action.";
                        break;
                    case RequestResult.RESULT_UNKNOWN:
                        break;
                }
            }
        }


        public delegate void HandleResponse( string data );

        public delegate void HandleError(RequestResult result, Uri requestUrl );

        public static void Perform( HttpWebRequest request,
                                    ID2LUserContext userContext,
                                    HandleResponse responseHandler,
                                    HandleError errorHandler,
                                    int retryAttempts = 0 ) {

            try {
                using (var response = (HttpWebResponse) request.GetResponse()) {
                    using (var stream = response.GetResponseStream()) {
                        using (var reader = new StreamReader( stream, Encoding.UTF8 )) {
                            string responseBody = reader.ReadToEnd();
                            responseHandler( responseBody );
                        }
                    }
                }
            }
            catch (WebException we) {
                var exceptionWrapper = new D2LWebException( we );
                var result = userContext.InterpretResult( exceptionWrapper );

                switch (result) {
                    case RequestResult.RESULT_INVALID_TIMESTAMP:
                        if (retryAttempts > 0) {
                            // re-create the request to ensure the new url accounts for calculated skew
                            Uri uri = userContext.CreateAuthenticatedUri( request.RequestUri.AbsoluteUri,
                                                                          request.Method );

                            request = (HttpWebRequest) WebRequest.Create( uri );

                            Perform( request, userContext, responseHandler, errorHandler, retryAttempts - 1 );
                        }
                        break;
                }
                errorHandler( result, request.RequestUri );
            }
            catch (ArgumentException) {
                errorHandler( RequestResult.RESULT_UNKNOWN, request.RequestUri );
            }
        }
    }
}