using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace GoogleBook {

    /// <summary>
    /// Summary description for GoogleBook.Api wrapper
    /// </summary>
    public class Api {
        public const int MAX_RESULTS = 4;

        static public GoogleBook.Volume Query( string query, int startIndex=0 ) {

            var uri = new Uri(
                "https://www.googleapis.com/books/v1/volumes"
                + "?q=" 
                + query 
                + "&startIndex=" 
                + startIndex
                + "&maxResults="
                + MAX_RESULTS);

            var request = ( HttpWebRequest )WebRequest.Create( uri );
            request.Method = "GET";

            var volume = new GoogleBook.Volume { TotalItems = 0 };

            try {
                using( var response = ( HttpWebResponse )request.GetResponse() ) {
                    using( var stream = response.GetResponseStream() ) {
                        using( var reader = new StreamReader( stream, Encoding.UTF8 ) ) {

                            string responseBody = reader.ReadToEnd();

                            var serializer = new JavaScriptSerializer();
                            volume = serializer.Deserialize<GoogleBook.Volume>( responseBody );
                        }
                    }
                }
            } catch (WebException) {}

            if( volume.items == null ) {
                volume.items = new GoogleBook.Item[0];
            }

            return volume;
        }
    }
}