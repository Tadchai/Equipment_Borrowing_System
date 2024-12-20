using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ItemClassification
    {
        [Key]
        public int ItemClassificationId { get; set; }
        public string Name { get; set; }
        public int ItemCategoryId { get; set; } // Foreign Key
    }
}