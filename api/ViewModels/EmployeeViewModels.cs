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
        public int EmployeeId { get; set;}
        public string FullName { get; set; }
        public List<RequisitionedItemResponse> RequisitionedItems { get; set; }
    }
    public class GetPaginatedEmployee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
    }
    public class GetSearchEmployee
    {
        public List<GetPaginatedEmployee> Data { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
    }
    public class SearchEmployeeRequest
    {
        public string? Name { get; set; }
        public int Page { get; set; } 
        public int PageSize { get; set;} 
    }
    public class DeleteEmployeeRequest
    {
        public int Id { get; set; }
    }

}