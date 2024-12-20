using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public class CreateEmployeeRequest
    {


        public string FullName { get; set; }
    }
    public class GetByIdResponse{
        public int Id { get; set;}
        public string FullName { get; set; }
    }
}