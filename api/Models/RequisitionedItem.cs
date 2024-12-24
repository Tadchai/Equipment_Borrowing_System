using System;
using System.Collections.Generic;

namespace api.Models;

public partial class RequisitionedItem
{
    public int RequisitionId { get; set; }

    public int EmployeeId { get; set; }

    public int ItemInstanceId { get; set; }

    public DateTime RequisitonDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ItemInstance ItemInstance { get; set; } = null!;

    public virtual ICollection<ItemInstance> ItemInstances { get; set; } = new List<ItemInstance>();
}
