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
    [Route("item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public ItemController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var Items = await _context.ItemCategories.ToListAsync();

            var data = Items.Select(i => new GetByIdItemResponse
            {
                Id = i.ItemCategoryId,
                Name = i.Name,
            }).ToList();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.ItemCategories.FirstOrDefaultAsync(i => i.ItemCategoryId == id);
            if (item == null)
            {
                return NotFound();
            }
            var data = new GetByIdItemResponse
            {
                Id = item.ItemCategoryId,
                Name = item.Name
            };
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest input)
        {
            if (string.IsNullOrEmpty(input.Name))
            {
                return BadRequest("ItemName is required");
            }
            var AnyName = await _context.ItemCategories.AnyAsync(i => i.Name == input.Name);
            if (AnyName)
            {
                return Conflict("Name already exists!!");
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var item = new ItemCategory
                    {
                        Name = input.Name,
                        CreateDate = DateTime.Now
                    };

                    await _context.ItemCategories.AddAsync(item);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var data = new GetByIdItemResponse
                    {
                        Id = item.ItemCategoryId,
                        Name = item.Name
                    };

                    return CreatedAtAction(nameof(GetById), new { id = item.ItemCategoryId }, data);
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateItemRequest Items)
        {
            var item = await _context.ItemCategories.FirstOrDefaultAsync(x => x.ItemCategoryId == id);
            if (item == null)
            {
                return NotFound("Item not found");
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    item.Name = Items.Name;
                    item.UpdateDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var data = new GetByIdItemResponse
                    {
                        Id = item.ItemCategoryId,
                        Name = item.Name
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
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ItemModel = await _context.ItemCategories.FirstOrDefaultAsync(x => x.ItemCategoryId == id);

            if (ItemModel == null)
            {
                return NotFound();
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.ItemCategories.Remove(ItemModel);
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
        [Route("search/{itemname}")]
        public async Task<IActionResult> Search([FromRoute] string itemname)
        {
            var itemsQuery = _context.ItemCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(itemname))
            {
                itemsQuery = itemsQuery.Where(x => x.Name.Contains(itemname));
            }

            var data = await itemsQuery
                .Select(i => new GetByIdItemResponse
                {
                    Id = i.ItemCategoryId,
                    Name = i.Name,
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}