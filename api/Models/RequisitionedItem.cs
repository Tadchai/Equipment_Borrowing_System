using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class RequisitionedItem
    {
        [Key]
        public int RequisitionId { get; set; }
        public int EmployeeId { get; set; } // Foreign Keys
        public int ItemInstanceId { get; set; } // Foreign Keys
        public DateTime RequisitionDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}