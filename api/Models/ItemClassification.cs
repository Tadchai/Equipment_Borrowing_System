using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ItemClassification
{
    public int ItemClassificationId { get; set; }

    public string Name { get; set; } = null!;

    public int ItemCategoryId { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ItemCategory ItemCategory { get; set; } = null!;

    public virtual ICollection<ItemInstance> ItemInstances { get; set; } = new List<ItemInstance>();
}
