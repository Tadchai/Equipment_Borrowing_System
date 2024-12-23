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
        public List<ItemClassificationRequest> ItemClassifications { get; set; }
    }
    public class ItemClassificationRequest
    {
        public string Name { get; set; }
        public List<ItemInstanceRequest> ItemInstances { get; set; }
    }
    public class ItemInstanceRequest
    {
        public string AssetId { get; set; }
    }

    //UpdateItemRequest
    public class UpdateItemRequest
    {
        public List<UpdateCategoryResponse> CategoryResponse { get; set; }
        public List<UpdateClassificationResponse> ClassificationResponse { get; set; }
        public List<UpdateInstanceResponse> InstanceResponse { get; set; }
    }
    public class UpdateCategoryResponse
    {
        public int CategoryId { get; set;}
        public string CategoryName { get; set; }
    }
    public class UpdateClassificationResponse
    {
        public int ClassificationId { get; set;}
        public string ClassificationName { get; set; }
    }
    public class UpdateInstanceResponse
    {
        public int InstanceId { get; set;}
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
        public List<ClassificationResponse> ItemClassifications { get; set; }
    }
    
    public class ClassificationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InstanceResponse> ItemInstances { get; set; }
    }

    public class InstanceResponse
    {
        public int Id { get; set; }
        public string AssetId { get; set; }
    }

    //DeleteRequest
    public class DeleteItemRequest
    {
        public List<int> CategoryId { get; set; }
        public List<int> ClassificationId { get; set; }
        public List<int> InstanceId { get; set; }
    }


}