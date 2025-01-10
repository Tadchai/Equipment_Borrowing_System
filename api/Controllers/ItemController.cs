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
            try
            {
                var itemCategory = await _context.ItemCategories.SingleAsync(ic => ic.ItemCategoryId == id);
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
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return new JsonResult(new MessageResponse { Message = "Item Category Name is null, empty, or whitespace.", StatusCode = HttpStatusCode.BadRequest });
            }
            var hasName = await _context.ItemCategories.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.Name);
            if (hasName)
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
                        CreateDate = DateTime.Now
                    };
                    await _context.ItemCategories.AddAsync(itemCategory);
                    await _context.SaveChangesAsync();

                    foreach (var classificationRequest in input.ItemClassifications)
                    {
                        var itemClassification = new ItemClassification
                        {
                            ItemCategoryId = itemCategory.ItemCategoryId,
                            Name = classificationRequest.Name,
                            CreateDate = DateTime.Now
                        };
                        await _context.ItemClassifications.AddAsync(itemClassification);
                        await _context.SaveChangesAsync();

                        foreach (var instanceRequest in classificationRequest.ItemInstances)
                        {
                            var instance = new ItemInstance
                            {
                                ItemClassificationId = itemClassification.ItemClassificationId,
                                AssetId = instanceRequest.AssetId,
                                CreateDate = DateTime.Now
                            };
                            await _context.ItemInstances.AddAsync(instance);
                        }
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
            var hasName = await _context.ItemCategories.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.Name && e.ItemCategoryId != input.Id);
            if (hasName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
            }

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
                            await _context.SaveChangesAsync();

                            classification.Id = newDbClassification.ItemClassificationId;

                            foreach (var newinstance in classification.ItemInstances)
                            {
                                var newDBInstance = new ItemInstance
                                {
                                    ItemClassificationId = newDbClassification.ItemClassificationId,
                                    AssetId = newinstance.AssetId,
                                    CreateDate = DateTime.Now
                                };
                                await _context.ItemInstances.AddAsync(newDBInstance);
                                await _context.SaveChangesAsync();

                                newinstance.Id = newDBInstance.ItemInstanceId;
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
                                        ItemClassificationId = oldClassification.ItemClassificationId,
                                        AssetId = instance.AssetId,
                                        CreateDate = DateTime.Now
                                    };
                                    await _context.ItemInstances.AddAsync(newDbInstance);
                                    await _context.SaveChangesAsync();

                                    instance.Id = newDbInstance.ItemInstanceId;
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
                    await _context.SaveChangesAsync();
                    var oldClassifications = await _context.ItemClassifications.Where(i => i.ItemCategoryId == input.Id).ToListAsync();
                    foreach (var classification in oldClassifications)
                    {
                        var oldInstance = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                        classification.ItemInstances = oldInstance;
                    }

                    foreach (var dbClass in oldClassifications)
                    {
                        var clientClass = input.ItemClassifications.SingleOrDefault(c => c.Id == dbClass.ItemClassificationId);
                        if (clientClass != null)
                        {
                            var missing = dbClass.ItemInstances.Where(dbInstance => !clientClass.ItemInstances.Any(clientInstance => clientInstance.Id == dbInstance.ItemInstanceId)).ToList();
                            foreach (var remove in missing)
                            {
                                var hasrequisition = await _context.RequisitionedItems.Where(ni => ni.ItemInstanceId == remove.ItemInstanceId).ToListAsync();
                                foreach (var requisition in hasrequisition)
                                {
                                    if (requisition.ReturnDate == null)
                                    {
                                        return new JsonResult(new MessageResponse { Message = "ItemInstance has not been returned and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
                                    }
                                    _context.RequisitionedItems.Remove(requisition);
                                }
                                _context.ItemInstances.Remove(remove);
                            }
                        }
                    }

                    var missingClassifications = oldClassifications.Where(dbClass => !input.ItemClassifications.Any(clientClass => clientClass.Id == dbClass.ItemClassificationId)).ToList();
                    foreach (var remove in missingClassifications)
                    {
                        var Instance = await _context.ItemInstances.Where(i => i.ItemClassificationId == remove.ItemClassificationId).ToListAsync();
                        foreach (var removeInstance in Instance)
                        {
                            var hasrequisition = await _context.RequisitionedItems.Where(ni => ni.ItemInstanceId == removeInstance.ItemInstanceId).ToListAsync();
                            foreach (var requisition in hasrequisition)
                            {
                                if (requisition.ReturnDate == null)
                                {
                                    return new JsonResult(new MessageResponse { Message = "ItemInstance has not been returned and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
                                }
                                _context.RequisitionedItems.Remove(requisition);
                            }
                            _context.ItemInstances.Remove(removeInstance);
                        }
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
        public async Task<IActionResult> Delete([FromBody] ItemDeleteRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemCategory = await _context.ItemCategories.SingleAsync(i => i.ItemCategoryId == input.Id);
                    var hasClassification = await _context.ItemClassifications.Where(i => i.ItemCategoryId == input.Id).ToListAsync();
                    foreach (var classification in hasClassification)
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
                Items = _context.ItemCategories.Where(e => e.Name.Contains(input.Name));
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
            try
            {
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
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }
    }
}