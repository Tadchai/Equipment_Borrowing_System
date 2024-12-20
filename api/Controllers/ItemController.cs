using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/item")]
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

            return Ok(Items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var Items = await _context.ItemCategories.FindAsync(id);

            if (Items == null)
            {
                return NotFound();
            }
            return Ok(Items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemCategory Items)
        {
            await _context.ItemCategories.AddAsync(Items);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = Items.ItemCategoryId }, Items);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ItemCategory Items)
        {
            var ItemModel = await _context.ItemCategories.FirstOrDefaultAsync(x => x.ItemCategoryId == id);
            if (ItemModel == null)
            {
                return NotFound();
            }
            ItemModel.Name = Items.Name;

            await _context.SaveChangesAsync();

            return Ok(ItemModel);

        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ItemModel = await _context.ItemCategories.FirstOrDefaultAsync(x => x.ItemCategoryId == id);

            if (ItemModel == null)
            {
                return NotFound();
            }
            _context.ItemCategories.Remove(ItemModel);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        [Route("search/{itemname}")]
        public async Task<IActionResult> Search([FromRoute] string itemname)
        {
            var Item = _context.ItemCategories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(itemname))
            {
                Item = Item.Where(x => x.Name.Contains(itemname));
            }
            await Item.ToListAsync();
            return Ok(Item);
        }
    }
}