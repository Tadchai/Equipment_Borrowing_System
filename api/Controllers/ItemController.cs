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
        public async Task<IActionResult> GetAll()
        {
            var Items = await _context.ItemCategories.ToListAsync();

            var data = Items.Select(i => new GetAllItemResponse
            {
                Id = i.ItemCategoryId,
                Name = i.Name
            }).ToList();

            return new JsonResult(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            // ดึงข้อมูลทั้งหมดที่เกี่ยวข้อง
            var items = await (from category in _context.ItemCategories
                               join classification in _context.ItemClassifications on category.ItemCategoryId equals classification.ItemCategoryId
                               join instance in _context.ItemInstances on classification.ItemClassificationId equals instance.ItemClassificationId
                               where category.ItemCategoryId == id
                               select new
                               {
                                   CategoryId = category.ItemCategoryId,
                                   CategoryName = category.Name,
                                   ClassificationId = classification.ItemClassificationId,
                                   ClassificationName = classification.Name,
                                   InstanceId = instance.ItemInstanceId,
                                   AssetId = instance.AssetId
                               }).ToListAsync();

            if (items == null || !items.Any())
            {
                return NotFound("No items found.");
            }

            var classifications = items
                .GroupBy(i => new { i.ClassificationId, i.ClassificationName })
                .Select(group => new ClassificationResponse
                {
                    Id = group.Key.ClassificationId,
                    Name = group.Key.ClassificationName,
                    ItemInstances = group.Select(instance => new InstanceResponse
                    {
                        Id = instance.InstanceId,
                        AssetId = instance.AssetId
                    }).ToList()
                }).ToList();

            var data = new GetByIdItemResponse
            {
                Id = items.First().CategoryId, 
                Name = items.First().CategoryName, 
                ItemClassifications = classifications
            };

            return new JsonResult(data);
        }




        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingCategory = await _context.ItemCategories.FirstOrDefaultAsync(c => c.Name == input.Name);
                    ItemCategory itemCategory;
                    if (existingCategory != null)
                    {
                        itemCategory = existingCategory;
                    }
                    else
                    {
                        itemCategory = new ItemCategory
                        {
                            Name = input.Name,
                            CreateDate = DateTime.Now,
                        };
                        await _context.ItemCategories.AddAsync(itemCategory);
                        await _context.SaveChangesAsync();
                    }

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
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new MessageResponse { Message = "Items Create successfully.", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
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
                    foreach (var category in input.CategoryRequest)
                    {
                        var Category = await _context.ItemCategories.SingleOrDefaultAsync(c => c.ItemCategoryId == category.CategoryId);
                        if (Category != null)
                        {
                            Category.Name = category.CategoryName;
                            Category.UpdateDate = DateTime.Now;
                        }
                    }
                    foreach (var classification in input.ClassificationRequest)
                    {
                        var Classification = await _context.ItemClassifications.SingleOrDefaultAsync(c => c.ItemClassificationId == classification.ClassificationId);
                        if (Classification != null)
                        {
                            Classification.Name = classification.ClassificationName;
                            Classification.UpdateDate = DateTime.Now;
                        }
                    }
                    foreach (var instance in input.InstanceRequest)
                    {
                        var Instance = await _context.ItemInstances.SingleOrDefaultAsync(c => c.ItemInstanceId == instance.InstanceId);
                        if (Instance != null)
                        {
                            Instance.AssetId = instance.AssetId;
                            Instance.UpdateDate = DateTime.Now;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new MessageResponse { Message = "Items update successfully.", StatusCode = 200 }); ;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteItemRequest request)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var categoryId in request.CategoryId)
                    {
                        var hasClassifications = await _context.ItemClassifications.AnyAsync(c => c.ItemCategoryId == categoryId);
                        if (hasClassifications)
                        {
                            return BadRequest(new MessageResponse { Message = "ไม่สามารถลบ Categories ที่มี Classificationsอยู่", StatusCode = 200 });
                        }

                        var category = await _context.ItemCategories.SingleOrDefaultAsync(c => c.ItemCategoryId == categoryId);
                        if (category != null)
                        {
                            _context.ItemCategories.Remove(category);
                        }
                    }
                    foreach (var classificationId in request.ClassificationId)
                    {
                        var hasInstancesRequisition = await _context.ItemInstances.AnyAsync(i => i.ItemClassificationId == classificationId && i.RequisitionId != null);
                        if (hasInstancesRequisition)
                        {
                            return BadRequest(new MessageResponse { Message = "ไม่สามารถลบ Classifications ที่มี Instances ที่มี requisitionId อยู่", StatusCode = 200 });
                        }

                        var classification = await _context.ItemClassifications.SingleOrDefaultAsync(c => c.ItemClassificationId == classificationId);
                        if (classification != null)
                        {
                            _context.ItemClassifications.Remove(classification);
                        }
                    }
                    foreach (var instanceId in request.InstanceId)
                    {
                        var hasRequisition = await _context.ItemInstances.AnyAsync(i => i.ItemInstanceId == instanceId && i.RequisitionId != null);
                        if (hasRequisition)
                        {
                            return BadRequest(new MessageResponse { Message = "ไม่สามารถลบ Instances ที่มี requisitionId อยู่", StatusCode = 200 });
                        }

                        var instance = await _context.ItemInstances.SingleOrDefaultAsync(i => i.ItemInstanceId == instanceId);
                        if (instance != null)
                        {
                            _context.ItemInstances.Remove(instance);
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });//Id

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPage(int page = 0, int pageSize = 10)
        {
            int skip = page * pageSize;
            int totalItems = await _context.ItemCategories.CountAsync();
            var paginatedItems = await _context.ItemCategories.Skip(skip).Take(pageSize).ToListAsync();
            var data = paginatedItems.Select(i => new PaginationItemResponse
            {
                ItemCategoryId = i.ItemCategoryId,
                Name = i.Name
            }).ToList();

            var response = new
            {
                Data = data,
                pageIndex = page,
                pageSize = pageSize,
                rowCount = totalItems,

            };
            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? name)
        {
            List<ItemCategory> items;
            if (name == null)
            {
                items = await _context.ItemCategories.ToListAsync();
            }
            else
            {
                items = await _context.ItemCategories.Where(i => i.Name.Contains(name)).ToListAsync();
            }
            var data = items.Select(i => new SearchResponse
            {
                ItemCategoryId = i.ItemCategoryId,
                Name = i.Name
            }).ToList();

            return new JsonResult(data);
        }
    }
}