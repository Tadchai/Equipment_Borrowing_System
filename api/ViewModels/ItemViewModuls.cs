using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public class CreateItemRequest
    {
        public string Name { get; set; }
    }
    public class UpdateItemRequest
    {
        public string Name { get; set; }
    }
    public class GetByIdItemResponse{
        public int Id { get; set;}
        public string Name { get; set; }
    }

}