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
    [Route("employee")]
    [ApiController]
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

            var data = employees.Select(e => new GetByIdResponse
            {
                Id = e.EmployeeId,
                FullName = e.Name
            }).ToList();

            return new JsonResult(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }
            var data = new GetByIdResponse
            {
                Id = employee.EmployeeId,
                FullName = employee.Name
            };
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest input)
        {
            if (string.IsNullOrEmpty(input.FullName))
            {
                return BadRequest("FullName is required.");
            }
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

                    var data = new GetByIdResponse
                    {
                        Id = employee.EmployeeId,
                        FullName = employee.Name
                    };

                    return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, data);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        [Route("update/{id}")]
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

                    var data = new GetByIdResponse
                    {
                        Id = employee.EmployeeId,
                        FullName = employee.Name
                    };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }


        [HttpPost]
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employeeModel = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (employeeModel == null)
            {
                return NotFound();
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Employees.Remove(employeeModel);
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
        [Route("search/{name}")]
        public async Task<IActionResult> Search(string name)
        {
            var employee = _context.Employees.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                employee = employee.Where(x => x.Name.Contains(name));
            }
            await employee.ToListAsync();
            
            var data = employee.Select(e => new GetByIdResponse
            {
                Id = e.EmployeeId,
                FullName = e.Name
            }).ToList();

            return Ok(data);
        }
    }
}