using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Barcode { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? ShopId { get; set; }
    public int? CostPrice { get; set; }
    public int? RetailPrice { get; set; }
    public int? StockQty { get; set; }
    public int? DiscountPercent { get; set; }
    public int? GuaranteeMonths { get; set; }
    public int? ImageId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Note { get; set; }
    public int? SoldQty { get; set; }
    public int? Status { get; set; }

    public Category? Category { get; set; }
    public Shop? Shop { get; set; }
    public ImageFile? ImageFile { get; set; }
}

