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
        public async Task<ActionResult> GetById([FromQuery] int id)
        {
            var itemCategory = await _context.ItemCategories.SingleOrDefaultAsync(ic => ic.ItemCategoryId == id);
            if (itemCategory == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = 404 });
            }

            var response = new GetByIdItemResponse
            {
                Id = itemCategory.ItemCategoryId,
                Name = itemCategory.Name,
                ItemClassifications = new List<ClassificationResponse>()
            };

            var classifications = await _context.ItemClassifications.Where(c => c.ItemCategoryId == id).ToListAsync();
            foreach (var classification in classifications)
            {
                var classificationResponse = new ClassificationResponse
                {
                    Id = classification.ItemClassificationId,
                    Name = classification.Name,
                    ItemInstances = new List<InstanceResponse>()
                };

                var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                foreach (var instance in instances)
                {
                    var requisition = await (from e in _context.Employees
                                             join r in _context.RequisitionedItems on e.EmployeeId equals r.EmployeeId
                                             where r.ItemInstanceId == instance.ItemInstanceId && r.ReturnDate == null
                                             select new { EmployeeId = r.EmployeeId, EmployeeName = r.Employee.Name }).SingleOrDefaultAsync();

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
            if (input.Name == string.Empty)
            {
                return new JsonResult(new MessageResponse { Message = "Item Category Name is empty", StatusCode = 409 });
            }
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
                        ItemClassifications = new List<ItemClassification>()
                    };
                    await _context.ItemCategories.AddAsync(itemCategory);

                    foreach (var classificationRequest in input.ItemClassifications)
                    {
                        var itemClassification = new ItemClassification
                        {
                            Name = classificationRequest.Name,
                            CreateDate = DateTime.Now,
                            ItemInstances = new List<ItemInstance>()
                        };

                        foreach (var instanceRequest in classificationRequest.ItemInstances)
                        {
                            var instance = new ItemInstance
                            {
                                AssetId = instanceRequest.AssetId,
                                CreateDate = DateTime.Now
                            };
                            itemClassification.ItemInstances.Add(instance);
                        }
                        itemCategory.ItemClassifications.Add(itemClassification);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new JsonResult(new MessageResponse { Id = itemCategory.ItemCategoryId, Message = "Items Create successfully.", StatusCode = 201 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }
        [HttpPost]
        public async Task<JsonResult> Update([FromBody] ItemUpdateRequest input)
        {
            var olditem = await _context.ItemCategories.SingleOrDefaultAsync(i => i.ItemCategoryId == input.Id);
            if (olditem == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = 404 });
            }
            var anyname = await _context.ItemCategories.AnyAsync(i => i.Name == input.Name && i.ItemCategoryId != input.Id);
            if (anyname)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = 409 });
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    olditem.Name = input.Name;
                    olditem.UpdateDate = DateTime.Now;

                    var oldClassifications = await _context.ItemClassifications.Where(i => i.ItemCategoryId == olditem.ItemCategoryId).ToListAsync();

                    foreach (var newclassification in input.ItemClassifications)
                    {
                        var oldClassification = oldClassifications.SingleOrDefault(oc => oc.ItemClassificationId == newclassification.Id);
                        if (oldClassification == null)
                        {
                            var newDBClassification = new ItemClassification
                            {
                                ItemCategoryId = olditem.ItemCategoryId,
                                Name = newclassification.Name,
                                CreateDate = DateTime.Now,
                                ItemInstances = new List<ItemInstance>()
                            };
                            await _context.ItemClassifications.AddAsync(newDBClassification);

                            foreach (var newinstance in newclassification.ItemInstances)
                            {
                                var newDBInstance = new ItemInstance
                                {
                                    ItemClassificationId = newDBClassification.ItemClassificationId,
                                    AssetId = newinstance.AssetId,
                                    CreateDate = DateTime.Now
                                };
                                newDBClassification.ItemInstances.Add(newDBInstance);
                            }
                        }
                        else
                        {
                            oldClassification.Name = newclassification.Name;
                            oldClassification.UpdateDate = DateTime.Now;

                            var oldInstances = await _context.ItemInstances.Where(i => i.ItemClassificationId == oldClassification.ItemClassificationId).ToListAsync();

                            foreach (var newinstance in newclassification.ItemInstances)
                            {
                                var oldInstance = oldInstances.SingleOrDefault(i => i.ItemInstanceId == newinstance.Id);
                                if (oldInstance == null)
                                {
                                    var newDBInstance = new ItemInstance
                                    {
                                        ItemClassificationId = oldClassification.ItemClassificationId,
                                        AssetId = newinstance.AssetId,
                                        CreateDate = DateTime.Now
                                    };
                                    await _context.ItemInstances.AddAsync(newDBInstance);
                                }
                                else
                                {
                                    oldInstance.AssetId = newinstance.AssetId;
                                    oldInstance.UpdateDate = DateTime.Now;
                                }
                            }

                            var instancesRemove = (from o in oldInstances
                                                   join n in newclassification.ItemInstances on o.ItemInstanceId equals n.Id into matched
                                                   from m in matched.DefaultIfEmpty()
                                                   where m == null
                                                   select o).ToList();
                            foreach (var item in instancesRemove)
                            {
                                var hasrequisition = await _context.RequisitionedItems.AnyAsync(ni => ni.ItemInstanceId == item.ItemInstanceId && ni.ReturnDate == null);
                                if (hasrequisition)
                                {
                                    return new JsonResult(new MessageResponse { Message = "ItemInstance is currently borrowing, so it cannot be deleted.", StatusCode = 400 });
                                }
                                _context.ItemInstances.Remove(item); 
                            }
                        }
                    }

                    var classificationsToRemove = (from o in oldClassifications
                                                   join n in input.ItemClassifications on o.ItemClassificationId equals n.Id into matched
                                                   from m in matched.DefaultIfEmpty()
                                                   where m == null
                                                   select o).ToList();
                    foreach (var classification in classificationsToRemove)
                    {
                        var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();

                        foreach (var instance in instances)
                        {
                            var hasrequisition = await _context.RequisitionedItems.AnyAsync(ni => ni.ItemInstanceId == instance.ItemInstanceId && ni.ReturnDate == null);
                            if (hasrequisition)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemInstance is currently borrowing, so it cannot be deleted.", StatusCode = 400 });
                            }
                            _context.ItemInstances.Remove(instance);
                        }
                        _context.ItemClassifications.Remove(classification);
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
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var itemCategory = await _context.ItemCategories.SingleOrDefaultAsync(i => i.ItemCategoryId == id);
            if (itemCategory == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategory not found.", StatusCode = 404 });
            }

            var hasclassification = await _context.ItemClassifications.AnyAsync(i => i.ItemCategoryId == id);
            if (hasclassification)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategory has ItemClassifications, so it cannot be deleted.", StatusCode = 400 });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.ItemCategories.Remove(itemCategory);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "ItemCategory deleted successfully.", StatusCode = 200 });
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
        public async Task<IActionResult> GetFreeItems()
        {
            var requisition = await (from ii in _context.ItemInstances
                                     join ic in _context.ItemClassifications on ii.ItemClassificationId equals ic.ItemClassificationId
                                     where ii.RequisitionId == null
                                     select new { AssetId = ii.AssetId, ClassificationName = ic.Name, ItemInstanceId = ii.ItemInstanceId }).ToListAsync();

            var data = requisition.Select(r => new FreeItemResponse
            {
                AssetId = r.AssetId,
                ClassificationName = r.ClassificationName,
                ItemInstanceId = r.ItemInstanceId,
            });

            return new JsonResult(data);
        }

        [HttpGet]
        public async Task<IActionResult> History([FromQuery] int input)
        {
            var history = await (from r in _context.RequisitionedItems
                                 join e in _context.Employees on r.EmployeeId equals e.EmployeeId
                                 where r.ItemInstanceId == input
                                 select new { Name = e.Name, RequisitionDate = r.RequisitonDate, ReturnDate = r.ReturnDate }).ToListAsync();

            var data = history.Select(h => new ItemHistoryResponse
            {
                EmployeeName = h.Name,
                RequisitonDate = h.RequisitionDate,
                ReturnDate = h.ReturnDate
            });

            return new JsonResult(data);
        }
    }
}