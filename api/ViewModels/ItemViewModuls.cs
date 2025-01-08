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
        public List<ItemClassificationRequest> ItemClassifications { get; set; } = new List<ItemClassificationRequest>();
    }
    public class ItemClassificationRequest
    {
        public string Name { get; set; }
        public List<ItemInstanceRequest> ItemInstances { get; set; } = new List<ItemInstanceRequest>();
    }
    public class ItemInstanceRequest
    {
        public string AssetId { get; set; }
    }

    public class GetByIdItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ClassificationResponse>? ItemClassifications { get; set; }
    }

    public class ClassificationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InstanceResponse>? ItemInstances { get; set; }
    }

    public class InstanceResponse
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
        public int? RequisitionEmployeeId { get; set; } = null;
        public string? RequisitionEmployeeName { get; set; } = null;
    }

    public class PaginationItemResponse
    {
        public int ItemCategoryId { get; set; }
        public string Name { get; set; }
    }

    public class SearchItemRequest
    {
        public string? Name { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; } 
    }
    public class SearchItemResponse
    {
        public List<PaginationItemResponse> Data { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
    }
    public class FreeItemResponse
    {
        public string AssetId { get; set; }
        public string ClassificationName { get; set; }
        public int ItemInstanceId { get; set; }
    }

    public class ItemHistoryResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime RequisitonDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public class ItemUpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ItemClassificationUpdateRequest> ItemClassifications { get; set; } = new List<ItemClassificationUpdateRequest>();
    }

    public class ItemClassificationUpdateRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public List<ItemInstanceUpdateRequest> ItemInstances { get; set; } = new List<ItemInstanceUpdateRequest>();
    }

    public class ItemInstanceUpdateRequest
    {
        public int? Id { get; set; }
        public string AssetId { get; set; }
    }

    public class ItemDeleteRequest
    {
        public int Id { get; set; }
    }
}