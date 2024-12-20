using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ItemCategory
    {
        [Key]
        public int ItemCategoryId { get; set; }
        public string Name { get; set; }
    }
}