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
    public class UpdateEmployeeRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
    public class GetByIdEmployeeResponse
    {
        public int Id { get; set;}
        public string FullName { get; set; }
        public List<RequisitionedItemResponse> RequisitionedItems { get; set; }
    }
    public class GetPaginatedEmployee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
    }

}