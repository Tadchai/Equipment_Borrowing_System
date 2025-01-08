using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public enum HttpStatusCode
    {
        OK = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        InternalServerError = 500
    }
    public class MessageResponse
    {
        public int? Id { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode{ get; set; }
    }
}