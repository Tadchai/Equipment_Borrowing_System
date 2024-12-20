using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public EmployeeController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var Employees = await _context.Employees.ToListAsync();

            return new JsonResult(Employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }
            var data = new GetByIdResponse();
            data.Id = employee.EmployeeId;
            data.FullName = employee.Name;
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest input)
        {
            var Models = new Employee();
            Models.Name = input.FullName;

            await _context.Employees.AddAsync(Models);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = input.EmployeeId }, input);
        }//

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Employee employee)
        {
            var employeeModel = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (employeeModel == null)
            {
                return NotFound();
            }
            employeeModel.Name = employee.Name;

            await _context.SaveChangesAsync();

            return Ok(employeeModel);

        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var employeeModel = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (employeeModel == null)
            {
                return NotFound();
            }
            _context.Employees.Remove(employeeModel);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        [Route("search/{name}")]
        public async Task<IActionResult> Search([FromRoute] string name)
        {
            var employee = _context.Employees.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                employee = employee.Where(x => x.Name.Contains(name));
            }
            await employee.ToListAsync();
            return Ok(employee);
        }
    }
}