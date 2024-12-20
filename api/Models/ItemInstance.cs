using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ItemInstance
{
    public int ItemInstanceId { get; set; }

    public string AssetId { get; set; } = null!;

    public int? RequisitionId { get; set; }

    public int ItemClassificationId { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ItemClassification ItemClassification { get; set; } = null!;

    public virtual RequisitionedItem? Requisition { get; set; }

    public virtual ICollection<RequisitionedItem> RequisitionedItems { get; set; } = new List<RequisitionedItem>();
}
