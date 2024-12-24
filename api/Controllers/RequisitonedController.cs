using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class RequisitonedController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public RequisitonedController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequest input)
        {
            var IdEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == input.EmployeeId);
            if (IdEmployee == null)
            {
                return Conflict(new MessageResponse { Message = "don't have Employee" , StatusCode = 200});
            }
            var IdInstance = await _context.ItemInstances.FirstOrDefaultAsync(i => i.ItemInstanceId == input.ItemInstanceId);
            if (IdInstance == null)
            {
                return Conflict(new MessageResponse { Message = "don't have ItemInstance" , StatusCode = 200});
            }
            if (IdInstance.RequisitionId != null)
            {
                return Conflict(new MessageResponse { Message = "ItemInstance has borrowed." , StatusCode = 200});
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var requisition = new RequisitionedItem
                    {
                        EmployeeId = input.EmployeeId,
                        ItemInstanceId = input.ItemInstanceId,
                        RequisitonDate = DateTime.Now,
                        CreateDate = DateTime.Now
                    };


                    await _context.RequisitionedItems.AddAsync(requisition);
                    await _context.SaveChangesAsync();

                    IdInstance.RequisitionId = requisition.RequisitionId;
                    _context.ItemInstances.Update(IdInstance);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new MessageResponse { Message = "Requisition Item successfully!" , StatusCode = 200});
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Return([FromBody] ReturnRequest input)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == input.EmployeeId);
            if (employee == null)
            {
                return Conflict(new MessageResponse { Message = "don't have Employees" , StatusCode = 200});
            }

            var requisition = await _context.RequisitionedItems.FirstOrDefaultAsync(r => r.RequisitionId == input.RequisitionId);
            if (requisition == null)
            {
                return Conflict(new MessageResponse { Message = "don't have Requisition", StatusCode = 200});
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    requisition.ReturnDate = DateTime.Now;
                    requisition.UpdateDate = DateTime.Now;

                    var itemInstance = await _context.ItemInstances.FirstOrDefaultAsync(i => i.RequisitionId == input.RequisitionId);
                    if (itemInstance == null)
                    {
                        return Conflict(new MessageResponse { Message = "don't have ItemInstance", StatusCode = 200});
                    }

                    itemInstance.RequisitionId = null;

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new MessageResponse { Message = "Return Item successfully!", StatusCode = 200});
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        // [HttpGet]
        // public async Task<IActionResult> Get()
        // {
        //     var requisition = await _context.
        // }



    }
}