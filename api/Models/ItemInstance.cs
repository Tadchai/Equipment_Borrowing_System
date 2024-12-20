using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ItemInstance
    {
        public int ItemInstanceId { get; set; }
        public string AssetId { get; set; }
        public int? RequisitionId { get; set; } // Foreign Key
        public int ItemClassificationId { get; set; } // Foreign Key
    }
}