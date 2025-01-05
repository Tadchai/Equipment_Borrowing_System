using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public class MessageResponse
    {
        public int? Id { get; set;}
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}