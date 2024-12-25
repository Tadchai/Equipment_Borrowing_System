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
            var IdEmployee = await _context.Employees.AnyAsync(e => e.EmployeeId == input.EmployeeId);
            if (!IdEmployee)
            {
                return new JsonResult(new MessageResponse { Message = "Employee not found.", StatusCode = 404 });
            }
            var IdInstance = await _context.ItemInstances.SingleOrDefaultAsync(i => i.ItemInstanceId == input.ItemInstanceId);
            if (IdInstance == null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemInstances not found.", StatusCode = 404 });
            }
            if (IdInstance.RequisitionId != null)
            {
                return new JsonResult(new MessageResponse { Message = "ItemInstance has borrowed.", StatusCode = 409 });
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
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Requisition Item successfully!", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Return([FromBody] ReturnRequest input)
        {
            var requisition = await _context.RequisitionedItems.SingleOrDefaultAsync(r => r.RequisitionId == input.RequisitionId);
            if (requisition == null)
            {
                return new JsonResult(new MessageResponse { Message = "Requisition not found.", StatusCode = 404 });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    requisition.ReturnDate = DateTime.Now;

                    var itemInstance = await _context.ItemInstances.SingleOrDefaultAsync(i => i.RequisitionId == input.RequisitionId);
                    if (itemInstance == null)
                    {
                        return new JsonResult(new MessageResponse { Message = "ItemInstance not found.", StatusCode = 404 });
                    }

                    itemInstance.RequisitionId = null;

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Return Item successfully!", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = 500 });
                }
            }
        }
    }
}