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
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees.ToListAsync();

            var data = employees.Select(e => new GetByIdEmployeeResponse
            {
                Id = e.EmployeeId,
                FullName = e.Name
            }).ToList();

            return new JsonResult(data);
        }//ใส่page

        [HttpGet]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var employee = await _context.Employees.SingleOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound(new MessageResponse { Message = "Employee not found.", StatusCode = 404 });
            }

            var data = await (from r in _context.RequisitionedItems
                              join i in _context.ItemInstances on r.ItemInstanceId equals i.ItemInstanceId
                              join ic in _context.ItemClassifications on i.ItemClassificationId equals ic.ItemClassificationId
                              join cat in _context.ItemCategories on ic.ItemCategoryId equals cat.ItemCategoryId
                              where r.EmployeeId == id
                              select new RequisitionedItemResponse
                              {
                                  AssetId = i.AssetId,
                                  ItemCategoryId = cat.ItemCategoryId,
                                  ItemCategoryName = cat.Name,
                                  ItemClassificationId = ic.ItemClassificationId,
                                  ItemClassificationName = ic.Name,
                                  InstanceId = i.ItemInstanceId,
                                  RequisitonDate = r.RequisitonDate,
                                  requisitionId = r.RequisitionId
                              }).ToListAsync();

            return new JsonResult(new
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
                return Conflict(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });
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

                    var data = new GetByIdEmployeeResponse
                    {
                        Id = employee.EmployeeId,
                        FullName = employee.Name
                    };

                    return Ok(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest updateRequest)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == updateRequest.Id);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    employee.Name = updateRequest.FullName;
                    employee.UpdateDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var data = new GetByIdEmployeeResponse
                    {
                        Id = employee.EmployeeId,
                        FullName = employee.Name
                    };

                    return Ok(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (employee == null)
            {
                return NotFound(new MessageResponse { Message = "Items deleted successfully.", StatusCode = 200 });
            }//เช็คการยืมหนังสือ
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { message = "Employee deleted successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPage(int page = 0, int pageSize = 10)
        {
            int skip = page * pageSize;
            int totalItems = await _context.Employees.CountAsync();
            var paginatedItems = await _context.Employees.Skip(skip).Take(pageSize).ToListAsync();
            var data = paginatedItems.Select(i => new GetPaginatedEmployee
            {
                EmployeeId = i.EmployeeId,
                Name = i.Name
            }).ToList();

            var response = new
            {
                Data = data,
                pageIndex = page,
                pageSize = pageSize,
                rowCount = totalItems,

            };
            return new JsonResult(response); //page searchรวมกัน
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? name)
        {
            List<Employee> employee;
            if (name == null)
            {
                employee = await _context.Employees.ToListAsync();
            }
            else
            {
                employee = await _context.Employees.Where(e => e.Name.Contains(name)).ToListAsync();
            }
            var data = employee.Select(e => new GetByIdEmployeeResponse
            {
                Id = e.EmployeeId,
                FullName = e.Name
            }).ToList();

            return new JsonResult(data);
        }
    }
}