using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public class RequisitionedItemResponse
    {
        public string AssetId { get; set; }
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; }
        public int ItemClassificationId { get; set; }
        public string ItemClassificationName { get; set; }
        public int InstanceId { get; set; }
        public DateTime RequisitonDate { get; set; }
        public int requisitionId { get; set; }

    }
    public class BorrowRequest
    {
        public int EmployeeId { get; set; }
        public int ItemInstanceId { get; set; }
    }

    public class ReturnRequest
    {
        public int RequisitionId { get; set; }
    }
}