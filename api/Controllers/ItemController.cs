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
            var item = await _context.ItemCategories.Include(c => c.ItemClassifications).ThenInclude(i => i.ItemInstances).FirstOrDefaultAsync(i => i.ItemCategoryId == id);
            if (item == null)
            {
                return NotFound();
            }
            var data = new GetByIdItemResponse
            {
                Id = item.ItemCategoryId,
                Name = item.Name,
                ItemClassifications = item.ItemClassifications.Select(c => new ClassificationResponse
                {
                    Id = c.ItemClassificationId,
                    Name = c.Name,
                    ItemInstances = c.ItemInstances.Select(i => new InstanceResponse
                    {
                        Id = i.ItemInstanceId,
                        AssetId = i.AssetId
                    }).ToList()
                }).ToList()
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
                    return Ok(new { Message = "Item created successfully!" });
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (input.CategoryResponse != null)
                {
                    foreach (var category in input.CategoryResponse)
                    {
                        var existingCategory = await _context.ItemCategories.FirstOrDefaultAsync(c => c.ItemCategoryId == category.CategoryId);
                        if (existingCategory != null)
                        {
                            existingCategory.Name = category.CategoryName;
                            existingCategory.UpdateDate = DateTime.Now;
                            _context.ItemCategories.Update(existingCategory);
                        }
                    }
                }

                if (input.ClassificationResponse != null)
                {
                    foreach (var classification in input.ClassificationResponse)
                    {
                        var existingClassification = await _context.ItemClassifications.FirstOrDefaultAsync(c => c.ItemClassificationId == classification.ClassificationId);
                        if (existingClassification != null)
                        {
                            existingClassification.Name = classification.ClassificationName;
                            existingClassification.UpdateDate = DateTime.Now;
                            _context.ItemClassifications.Update(existingClassification);
                        }
                    }
                }

                if (input.InstanceResponse != null)
                {
                    foreach (var instance in input.InstanceResponse)
                    {
                        var existingInstance = await _context.ItemInstances.FirstOrDefaultAsync(c => c.ItemInstanceId == instance.InstanceId);
                        if (existingInstance != null)
                        {
                            existingInstance.AssetId = instance.AssetId;
                            existingInstance.UpdateDate = DateTime.Now;
                            _context.ItemInstances.Update(existingInstance);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Items updated successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteItemRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (request.CategoryId.Any())
                {
                    var hasLinkedClassifications = await _context.ItemClassifications.AnyAsync(c => request.CategoryId.Contains(c.ItemCategoryId));
                    if (hasLinkedClassifications)
                    {
                        return BadRequest(new { error = "ไม่สามารถลบ Categories ได้เนื่องจากมี Classifications เชื่อมโยงอยู่" });
                    }

                    var categories = await _context.ItemCategories.Where(c => request.CategoryId.Contains(c.ItemCategoryId)).ToListAsync();
                    _context.ItemCategories.RemoveRange(categories);
                }

                if (request.ClassificationId.Any())
                {
                    var hasLinkedInstancesWithRequisition = await _context.ItemInstances.AnyAsync(i => request.ClassificationId.Contains(i.ItemClassificationId) && i.RequisitionId != null);

                    if (hasLinkedInstancesWithRequisition)
                    {
                        return BadRequest(new { error = "ไม่สามารถลบ Classifications ได้เนื่องจากมี Instances ที่มี requisitionId เชื่อมโยงอยู่" });
                    }

                    var classifications = await _context.ItemClassifications.Where(c => request.ClassificationId.Contains(c.ItemClassificationId)).ToListAsync();
                    _context.ItemClassifications.RemoveRange(classifications);
                }

                if (request.InstanceId.Any())
                {
                    var hasRequisition = await _context.ItemInstances.AnyAsync(i => request.InstanceId.Contains(i.ItemInstanceId) && i.RequisitionId != null);
                    if (hasRequisition)
                    {
                        return BadRequest(new { error = "ไม่สามารถลบ Instances ได้เนื่องจากมี requisitionId อยู่" });
                    }

                    var instances = await _context.ItemInstances.Where(i => request.InstanceId.Contains(i.ItemInstanceId)).ToListAsync();
                    _context.ItemInstances.RemoveRange(instances);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "ลบข้อมูลเรียบร้อยแล้ว" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Search([FromRoute] string itemname)
        {
            var itemsQuery = _context.ItemCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(itemname))
            {
                itemsQuery = itemsQuery.Where(x => x.Name.Contains(itemname));
            }

            var data = await itemsQuery
                .Select(i => new GetByIdItemResponse
                {
                    Id = i.ItemCategoryId,
                    Name = i.Name,
                })
                .ToListAsync();

            return new JsonResult(data);
        }
    }
}