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
                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = HttpStatusCode.NotFound });
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
                                             where r.RequisitionId == instance.RequisitionId
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
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return new JsonResult(new MessageResponse { Message = "Item Category Name is null, empty, or whitespace.", StatusCode = HttpStatusCode.Conflict });
            }
            var AnyName = await _context.ItemCategories.AnyAsync(e => e.Name == input.Name);
            if (AnyName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
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
                    return new JsonResult(new MessageResponse { Id = itemCategory.ItemCategoryId, Message = "Items Create successfully.", StatusCode = HttpStatusCode.Created });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ItemUpdateRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var oldCategory = await _context.ItemCategories.SingleAsync(i => i.ItemCategoryId == input.Id);
                    oldCategory.Name = input.Name;
                    oldCategory.UpdateDate = DateTime.Now;
                    foreach (var classification in input.ItemClassifications)
                    {
                        if (classification.Id == null)
                        {
                            var newDbClassification = new ItemClassification
                            {
                                ItemCategoryId = oldCategory.ItemCategoryId,
                                Name = classification.Name,
                                CreateDate = DateTime.Now,
                                ItemInstances = new List<ItemInstance>()
                            };
                            await _context.ItemClassifications.AddAsync(newDbClassification);

                            foreach (var newinstance in classification.ItemInstances)
                            {
                                var newDBInstance = new ItemInstance
                                {
                                    AssetId = newinstance.AssetId,
                                    CreateDate = DateTime.Now
                                };
                                newDbClassification.ItemInstances.Add(newDBInstance);
                            }
                        }
                        else
                        {
                            var oldClassification = await _context.ItemClassifications.SingleAsync(oc => oc.ItemClassificationId == classification.Id);
                            oldClassification.Name = classification.Name;
                            oldClassification.UpdateDate = DateTime.Now;

                            foreach (var instance in classification.ItemInstances)
                            {
                                if (instance.Id == null)
                                {
                                    var newDbInstance = new ItemInstance
                                    {
                                        AssetId = instance.AssetId,
                                        CreateDate = DateTime.Now
                                    };
                                    oldClassification.ItemInstances.Add(newDbInstance);
                                }
                                else
                                {
                                    var oldInstance = await _context.ItemInstances.SingleAsync(oi => oi.ItemInstanceId == instance.Id);
                                    oldInstance.AssetId = instance.AssetId;
                                    oldInstance.UpdateDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    var oldClassifications = await _context.ItemClassifications.Where(i => i.ItemCategoryId == input.Id).Select(classification => new ItemClassification
                    {
                        ItemClassificationId = classification.ItemClassificationId,
                        ItemCategoryId = classification.ItemCategoryId,
                        Name = classification.Name,
                        UpdateDate = classification.UpdateDate,
                        ItemInstances = _context.ItemInstances.Where(instance => instance.ItemClassificationId == classification.ItemClassificationId).ToList()
                    }).ToListAsync();

                    foreach (var dbClass in oldClassifications)
                    {
                        var clientClass = input.ItemClassifications.SingleOrDefault(c => c.Id == dbClass.ItemClassificationId);
                        if (clientClass != null)
                        {
                            var missing = dbClass.ItemInstances.Where(dbInstance => !clientClass.ItemInstances.Any(clientInstance => clientInstance.Id == dbInstance.ItemInstanceId)).ToList();
                            foreach (var remove in missing)
                            {
                                _context.ItemInstances.Remove(remove);
                            }
                        }
                    }

                    var missingClassifications = oldClassifications.Where(dbClass => !input.ItemClassifications.Any(clientClass => clientClass.Id == dbClass.ItemClassificationId)).ToList();
                    foreach (var remove in missingClassifications)
                    {
                        _context.ItemClassifications.Remove(remove);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Items update successfully.", StatusCode = HttpStatusCode.OK }); ;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<JsonResult> oldUpdate([FromBody] ItemUpdateRequest input)
        {
            var olditem = await _context.ItemCategories.SingleOrDefaultAsync(i => i.ItemCategoryId == input.Id);
            if (olditem == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategories not found.", StatusCode = HttpStatusCode.NotFound });
            }

            var anyname = await _context.ItemCategories.AnyAsync(i => i.Name == input.Name && i.ItemCategoryId != input.Id);
            if (anyname)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
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
                                var oldInstance = oldInstances.SingleOrDefault(i => i.ItemInstanceId == newinstance.Id);//single
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
                                                   select o).ToList();//ไปดู      database use await async ทั้งหมด

                            foreach (var item in instancesRemove)
                            {
                                var hasrequisition = await _context.RequisitionedItems.AnyAsync(ni => ni.ItemInstanceId == item.ItemInstanceId);
                                if (hasrequisition)
                                {
                                    return new JsonResult(new MessageResponse { Message = "ItemInstance has already been borrowed and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
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
                            var hasrequisition = await _context.RequisitionedItems.AnyAsync(ni => ni.ItemInstanceId == instance.ItemInstanceId);
                            if (hasrequisition)
                            {
                                return new JsonResult(new MessageResponse { Message = "ItemInstance has already been borrowed and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
                            }
                            _context.ItemInstances.Remove(instance);
                        }
                        _context.ItemClassifications.Remove(classification);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Items update successfully.", StatusCode = HttpStatusCode.OK }); ;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ItemDeleteRequest input)
        {
            var itemCategory = await _context.ItemCategories.SingleOrDefaultAsync(i => i.ItemCategoryId == input.Id);
            if (itemCategory == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemCategory not found.", StatusCode = HttpStatusCode.NotFound });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var hasclassification = await _context.ItemClassifications.Where(i => i.ItemCategoryId == input.Id).ToListAsync();
                    foreach (var classification in hasclassification)
                    {
                        var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                        foreach (var instance in instances)
                        {
                            var requisitions = await _context.RequisitionedItems.Where(i => i.ItemInstanceId == instance.ItemInstanceId).ToListAsync();
                            foreach (var requisition in requisitions)
                            {
                                if (requisition.ReturnDate == null)
                                {
                                    return new JsonResult(new MessageResponse { Message = "ItemInstance has not been returned and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
                                }
                                _context.RequisitionedItems.Remove(requisition);
                            }
                            _context.ItemInstances.Remove(instance);
                        }
                        _context.ItemClassifications.Remove(classification);
                    }
                    _context.ItemCategories.Remove(itemCategory);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "ItemCategory deleted successfully.", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchItemRequest input)
        {
            if (input.PageSize <= 0)
            {
                return new JsonResult(new MessageResponse { Message = "PageSize must be greater than or equal to 0.", StatusCode = HttpStatusCode.BadRequest });
            }

            int skip = input.Page * input.PageSize;
            IQueryable<ItemCategory> Items;

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                Items = _context.ItemCategories;
            }
            else
            {
                Items = _context.ItemCategories.Where(e => EF.Functions.Collate(e.Name, "utf8mb4_bin").Contains(input.Name));
            }

            int totalItems = await Items.CountAsync();
            var paginatedItems = await Items.Skip(skip).Take(input.PageSize).ToListAsync();

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
        public async Task<IActionResult> History([FromQuery] int id)
        {
            var itemInstance = await _context.ItemInstances.AnyAsync(i => i.ItemInstanceId == id);
            if (!itemInstance)
            {
                return new JsonResult(new MessageResponse { Message = "ItemInstance not found.", StatusCode = HttpStatusCode.NotFound });
            }

            var history = await (from r in _context.RequisitionedItems
                                 join e in _context.Employees on r.EmployeeId equals e.EmployeeId
                                 where r.ItemInstanceId == id
                                 select new { Id = e.EmployeeId, Name = e.Name, RequisitionDate = r.RequisitonDate, ReturnDate = r.ReturnDate }).ToListAsync();

            var data = history.Select(h => new ItemHistoryResponse
            {
                EmployeeId = h.Id,
                EmployeeName = h.Name,
                RequisitonDate = h.RequisitionDate,
                ReturnDate = h.ReturnDate
            });

            return new JsonResult(data);
        }
    }
}