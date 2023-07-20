using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Exceptions
{
    internal class HttpResponseUnsuccessfulException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; }

        public HttpResponseUnsuccessfulException(HttpResponseMessage responseMessage) : base()
        {
            ResponseMessage = responseMessage;
        }
    }
}
