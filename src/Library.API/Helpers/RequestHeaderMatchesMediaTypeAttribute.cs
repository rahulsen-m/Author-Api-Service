using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Library.API.Helpers
{
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string[] _mediaTypes;
        private readonly string _requestHeaderToMatch;

        // requestHeaderToMatch used check the request is supported 
        // mediaTypes array we are supporting
        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
            string[] mediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;
            _mediaTypes = mediaTypes;
        }

        // Constraints are grouped into stages by the value
        public int Order
        {
            get
            {
                // we return 0 as we are not changes the stage and keeping the default
                return 0;
            }
        }

        // If route attribute constraints is matched it will return true else false
        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            // if one of the media types matches, return true
            foreach (var mediaType in _mediaTypes)
            {
                var mediaTypeMatches = string.Equals(requestHeaders[_requestHeaderToMatch].ToString(),
                    mediaType, StringComparison.OrdinalIgnoreCase);

                if (mediaTypeMatches)
                {
                    return true;
                }
            }

            return false;
        }
    }
}