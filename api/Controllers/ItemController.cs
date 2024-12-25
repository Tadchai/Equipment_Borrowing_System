using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ItemController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public ItemController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetById(int id)
        {
            var itemCategory = await _context.ItemCategories.FirstOrDefaultAsync(ic => ic.ItemCategoryId == id);
            if (itemCategory == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = 404 });
            }

            var classifications = await _context.ItemClassifications.Where(c => c.ItemCategoryId == id).ToListAsync();
            var response = new GetByIdItemResponse
            {
                Id = itemCategory.ItemCategoryId,
                Name = itemCategory.Name,
                ItemClassifications = new List<ClassificationResponse>()
            };

            foreach (var classification in classifications)
            {
                var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                var classificationResponse = new ClassificationResponse
                {
                    Id = classification.ItemClassificationId,
                    Name = classification.Name,
                    ItemInstances = new List<InstanceResponse>()
                };

                foreach (var instance in instances)
                {
                    var requisition = await _context.RequisitionedItems.Where(r => r.ItemInstanceId == instance.ItemInstanceId).Select(r => new
                    {
                        EmployeeId = r.EmployeeId,
                        EmployeeName = r.Employee.Name
                    }).FirstOrDefaultAsync();

                    classificationResponse.ItemInstances.Add(new InstanceResponse
                    {
                        Id = instance.ItemInstanceId,
                        AssetId = instance.AssetId,
                        RequisitionEmployeeId = requisition?.EmployeeId,
                        RequisitionEmployeeName = requisition?.EmployeeName
                    });
                }
                response.ItemClassifications.Add(classificationResponse);
            }
            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest input)
        {
            var AnyName = await _context.ItemCategories.AnyAsync(e => e.Name == input.Name);
            if (AnyName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = 409 });
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemCategory = new ItemCategory
                    {
                        Name = input.Name,
                        CreateDate = DateTime.Now,
                    };
                    await _context.ItemCategories.AddAsync(itemCategory);
                    await _context.SaveChangesAsync();

                    if (input.ItemClassifications != null)
                    {
                        foreach (var ClassificationsRequest in input.ItemClassifications)
                        {
                            var existingclassification = await _context.ItemClassifications.FirstOrDefaultAsync(c => c.Name == ClassificationsRequest.Name);
                            ItemClassification itemClassifications;
                            if (existingclassification != null)
                            {
                                itemClassifications = existingclassification;
                            }
                            else
                            {
                                itemClassifications = new ItemClassification
                                {
                                    Name = ClassificationsRequest.Name,
                                    ItemCategoryId = itemCategory.ItemCategoryId,
                                    CreateDate = DateTime.Now
                                };
                                await _context.ItemClassifications.AddAsync(itemClassifications);
                                await _context.SaveChangesAsync();
                            }
                            if (ClassificationsRequest.ItemInstances != null)
                            {
                                foreach (var InstanceRequest in ClassificationsRequest.ItemInstances)
                                {
                                    var existingInstance = await _context.ItemInstances.FirstOrDefaultAsync(i => i.AssetId == InstanceRequest.AssetId);
                                    if (existingInstance == null)
                                    {
                                        var instance = new ItemInstance
                                        {
                                            AssetId = InstanceRequest.AssetId,
                                            ItemClassificationId = itemClassifications.ItemClassificationId,
                                            CreateDate = DateTime.Now
                                        };
                                        await _context.ItemInstances.AddAsync(instance);
                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new JsonResult(new MessageResponse { Message = "Items Create successfully.", StatusCode = 201 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateItemRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (input.CategoryRequest != null)
                    {
                        foreach (var category in input.CategoryRequest)
                        {
                            var AnyName = await _context.ItemCategories.AnyAsync(e => e.Name == category.CategoryName);
                            if (AnyName)
                            {
                                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = 409 });
                            }
                            var Category = await _context.ItemCategories.SingleOrDefaultAsync(c => c.ItemCategoryId == category.CategoryId);
                            if (Category == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = 404 });
                            }
                            Category.Name = category.CategoryName;
                            Category.UpdateDate = DateTime.Now;
                        }
                    }
                    if (input.ClassificationRequest != null)
                    {
                        foreach (var classification in input.ClassificationRequest)
                        {
                            var Classification = await _context.ItemClassifications.SingleOrDefaultAsync(c => c.ItemClassificationId == classification.ClassificationId);
                            if (Classification == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemClassifications not found.", StatusCode = 404 });
                            }
                            Classification.Name = classification.ClassificationName;
                            Classification.UpdateDate = DateTime.Now;
                        }
                    }
                    if (input.InstanceRequest != null)
                    {
                        foreach (var instance in input.InstanceRequest)
                        {
                            var Instance = await _context.ItemInstances.SingleOrDefaultAsync(c => c.ItemInstanceId == instance.InstanceId);
                            if (Instance == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemInstances not found.", StatusCode = 404 });
                            }
                            Instance.AssetId = instance.AssetId;
                            Instance.UpdateDate = DateTime.Now;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Items update successfully.", StatusCode = 200 }); ;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteItemRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (input.CategoryId != null)
                    {
                        foreach (var categoryId in input.CategoryId)
                        {
                            var category = await _context.ItemCategories.SingleOrDefaultAsync(c => c.ItemCategoryId == categoryId);
                            if (category == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = 404 });
                            }

                            var hasClassifications = await _context.ItemClassifications.AnyAsync(c => c.ItemCategoryId == categoryId);
                            if (hasClassifications)
                            {
                                return new JsonResult(new MessageResponse { Message = "Cannot delete Categories because Classifications exist.", StatusCode = 400 });
                            }

                            _context.ItemCategories.Remove(category);
                        }
                    }
                    if (input.ClassificationId != null)
                    {
                        foreach (var classificationId in input.ClassificationId)
                        {
                            var classification = await _context.ItemClassifications.SingleOrDefaultAsync(c => c.ItemClassificationId == classificationId);
                            if (classification == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemClassifications not found.", StatusCode = 404 });
                            }

                            var hasInstancesRequisition = await _context.ItemInstances.AnyAsync(i => i.ItemClassificationId == classificationId && i.RequisitionId != null);
                            if (hasInstancesRequisition)
                            {
                                return new JsonResult(new MessageResponse { Message = "This class cannot be deleted because Item is being borrowed.", StatusCode = 400 });
                            }

                            _context.ItemClassifications.Remove(classification);
                        }
                    }
                    if (input.InstanceId != null)
                    {
                        foreach (var instanceId in input.InstanceId)
                        {
                            var instance = await _context.ItemInstances.SingleOrDefaultAsync(i => i.ItemInstanceId == instanceId);
                            if (instance == null)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemClassifications not found.", StatusCode = 404 });
                            }

                            var hasRequisition = await _context.ItemInstances.AnyAsync(i => i.ItemInstanceId == instanceId && i.RequisitionId != null);
                            if (hasRequisition)
                            {
                                return new JsonResult(new MessageResponse { Message = "Item is being borrowed.", StatusCode = 400 });
                            }

                            _context.ItemInstances.Remove(instance);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchItemRequest input)
        {
            int skip = input.Page * input.PageSize;
            List<ItemCategory> Items;

            if (input.Name == null)
            {
                Items = await _context.ItemCategories.ToListAsync();
            }
            else
            {
                Items = await _context.ItemCategories.Where(e => e.Name.Contains(input.Name)).ToListAsync();
            }

            int totalItems = Items.Count();
            var paginatedItems = Items.Skip(skip).Take(input.PageSize).ToList();

            var data = paginatedItems.Select(i => new PaginationItemResponse
            {
                ItemCategoryId = i.ItemCategoryId,
                Name = i.Name
            }).ToList();

            var response = new SearchItemResponse
            {
                Data = data,
                PageIndex = input.Page,
                PageSize = input.PageSize,
                RowCount = totalItems,
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetFreeItems(int input)
        {
            var requisition = await (from ii in _context.ItemInstances
                                     join ic in _context.ItemClassifications on ii.ItemClassificationId equals ic.ItemClassificationId
                                     join ca in _context.ItemCategories on ic.ItemCategoryId equals ca.ItemCategoryId
                                     where ii.RequisitionId == null && ca.ItemCategoryId == input
                                     select new { AssetId = ii.AssetId, ClassificationName = ic.Name, ItemInstanceId = ii.ItemInstanceId }).ToListAsync();

            var data = requisition.Select(r => new FreeItemResponse
            {
                AssetId = r.AssetId,
                ClassificationName = r.ClassificationName,
                ItemInstanceId = r.ItemInstanceId,
            });

            return new JsonResult(data);
        }
    }
}