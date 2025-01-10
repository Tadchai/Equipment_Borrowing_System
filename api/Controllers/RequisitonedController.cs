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
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var IdInstance = await _context.ItemInstances.SingleAsync(i => i.ItemInstanceId == input.ItemInstanceId);
                    if (IdInstance.RequisitionId != null)
                    {
                        return new JsonResult(new MessageResponse { Message = "ItemInstance has borrowed.", StatusCode = HttpStatusCode.Conflict });
                    }

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

                    return new JsonResult(new MessageResponse { Message = "Requisition Item successfully!", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Return([FromBody] ReturnRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var requisition = await _context.RequisitionedItems.SingleAsync(r => r.RequisitionId == input.RequisitionId);
                    requisition.ReturnDate = DateTime.Now;

                    var itemInstance = await _context.ItemInstances.SingleAsync(i => i.RequisitionId == input.RequisitionId);
                    itemInstance.RequisitionId = null;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Return Item successfully!", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }
    }
}