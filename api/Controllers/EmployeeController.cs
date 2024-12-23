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
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }
            var data = new GetByIdEmployeeResponse
            {
                Id = employee.EmployeeId,
                FullName = employee.Name
            };
            return new JsonResult(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest input)
        {
            var AnyName = await _context.Employees.AnyAsync(e => e.Name == input.FullName);
            if (AnyName)
            {
                return Conflict("Name already exists!!");
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

                    return new JsonResult(data);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest updateRequest)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
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

                    return new JsonResult(data);
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
                return NotFound();
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return NoContent();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string name)
        {
            var employee = _context.Employees.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                employee = employee.Where(x => x.Name.Contains(name));
            }
            var data = await employee.Select(e => new GetByIdEmployeeResponse
            {
                Id = e.EmployeeId,
                FullName = e.Name
            }).ToListAsync();

            return new JsonResult(data);
        }
    }
}