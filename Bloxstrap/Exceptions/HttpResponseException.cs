using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Exceptions
{
    internal class HttpResponseException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; }

        public HttpResponseException(HttpResponseMessage responseMessage) 
            : base($"Could not connect to {responseMessage.RequestMessage!.RequestUri} because it returned HTTP {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase})")
        {
            ResponseMessage = responseMessage;
        }
    }
}
