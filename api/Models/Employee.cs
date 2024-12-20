using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ICollection<RequisitionedItem> RequisitionedItems { get; set; } = new List<RequisitionedItem>();
}
