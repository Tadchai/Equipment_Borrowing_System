using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public EmployeeController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var employee = await _context.Employees.SingleOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return new JsonResult(new MessageResponse { Message = "Employee not found.", StatusCode = 404 });
            }

            var data = await (from ri in _context.RequisitionedItems
                              join ii in _context.ItemInstances on ri.ItemInstanceId equals ii.ItemInstanceId
                              join ic in _context.ItemClassifications on ii.ItemClassificationId equals ic.ItemClassificationId
                              join cat in _context.ItemCategories on ic.ItemCategoryId equals cat.ItemCategoryId
                              where ri.EmployeeId == id && ri.ReturnDate == null
                              select new RequisitionedItemResponse {
                                  AssetId = ii.AssetId,
                                  ItemCategoryId = cat.ItemCategoryId,
                                  ItemCategoryName = cat.Name,
                                  ItemClassificationId = ic.ItemClassificationId,
                                  ItemClassificationName = ic.Name,
                                  InstanceId = ii.ItemInstanceId,
                                  RequisitonDate = ri.RequisitonDate,
                                  requisitionId = ri.RequisitionId
                              }).ToListAsync();

            return new JsonResult(new GetByIdEmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.Name,
                RequisitionedItems = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest input)
        {
            var AnyName = await _context.Employees.AnyAsync(e => e.Name == input.FullName);
            if (AnyName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = 409 });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var employee = new Employee
                    {
                        Name = input.FullName,
                        CreateDate = DateTime.Now
                    };

                    await _context.Employees.AddAsync(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Employee Created successfully.", StatusCode = 201 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest input)
        {
            var employee = await _context.Employees.SingleOrDefaultAsync(e => e.EmployeeId == input.Id);
            if (employee == null)
            {
                return new JsonResult(new MessageResponse { Message = "Employee not found.", StatusCode = 404 });
            }
            var AnyName = await _context.Employees.AnyAsync(e => e.Name == input.FullName);
            if (AnyName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = 409 });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    employee.Name = input.FullName;
                    employee.UpdateDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Employee Updated successfully.", StatusCode = 200 });
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
            var employee = await _context.Employees.SingleOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return new JsonResult(new MessageResponse { Message = "Employee not found.", StatusCode = 404 });
            }

            var requisition = await _context.RequisitionedItems.AnyAsync(e => e.EmployeeId == id && e.ReturnDate == null);
            if (requisition)
            {
                return new JsonResult(new MessageResponse { Message = "Employee is currently borrowing", StatusCode = 400 });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Employee deleted successfully.", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody]SearchEmployeeRequest input)
        {
            int skip = input.Page* input.PageSize;
            List<Employee> employee;

            if (input.Name == null)
            {
                employee = await _context.Employees.ToListAsync();
            }
            else
            {
                employee = await _context.Employees.Where(e => e.Name.Contains(input.Name)).ToListAsync();
            }

            int totalItems = employee.Count();
            var paginatedItems = employee.Skip(skip).Take(input.PageSize).ToList();

            var data = paginatedItems.Select(i => new GetPaginatedEmployee
            {
                EmployeeId = i.EmployeeId,
                Name = i.Name
            }).ToList();

            var response = new GetSearchEmployee
            {
                Data = data,
                PageIndex = input.Page,
                PageSize = input.PageSize,
                RowCount = totalItems,
            };

            return new JsonResult(response);
        }
    }
}