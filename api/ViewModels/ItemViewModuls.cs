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
        public List<ItemClassificationRequest>? ItemClassifications { get; set; }
    }
    public class ItemClassificationRequest
    {
        public string Name { get; set; }
        public List<ItemInstanceRequest>? ItemInstances { get; set; }
    }
    public class ItemInstanceRequest
    {
        public string AssetId { get; set; }
    }

    //UpdateItemRequest
    public class UpdateItemRequest
    {
        public List<UpdateCategoryRequest>? CategoryRequest { get; set; }
        public List<UpdateClassificationRequest>? ClassificationRequest { get; set; }
        public List<UpdateInstanceRequest>? InstanceRequest { get; set; }
    }
    public class UpdateCategoryRequest
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
    public class UpdateClassificationRequest
    {
        public int ClassificationId { get; set; }
        public string ClassificationName { get; set; }
    }
    public class UpdateInstanceRequest
    {
        public int InstanceId { get; set; }
        public string AssetId { get; set; }
    }

    //GetItemResponse
    public class GetAllItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
        public string? RequisitionEmployeeName { get; set;} = null;
    }

    //DeleteRequest
    public class DeleteItemRequest
    {
        public List<int>? CategoryId { get; set; }
        public List<int>? ClassificationId { get; set; }
        public List<int>? InstanceId { get; set; }
    }

    public class PaginationItemResponse
    {
        public int ItemCategoryId { get; set; }
        public string Name { get; set; }
    }

    public class SearchResponse
    {
        public int ItemCategoryId { get; set; }
        public string Name { get; set; }
    }
     public class SearchItemRequest
    {
        public string? Name { get; set; }
        public int Page { get; set; } 
        public int PageSize { get; set;} 
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
        public string AssetId {get; set; }
        public string ClassificationName { get; set; }
        public int ItemInstanceId {get; set; }
    }
    

}