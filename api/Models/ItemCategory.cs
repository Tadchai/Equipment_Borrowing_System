using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ItemCategory
{
    public int ItemCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ICollection<ItemClassification> ItemClassifications { get; set; } = new List<ItemClassification>();
}
