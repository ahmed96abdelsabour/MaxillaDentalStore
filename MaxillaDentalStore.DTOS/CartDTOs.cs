using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxillaDentalStore.DTOS
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalCartPrice => Items.Sum(ci => ci.TotalPrice);
    }


    public class AddToCartDto
    {
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public int Quantity { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
        public string? ItemNotes { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
    }

    /// <summary>
    /// Cart Summary - lightweight for user details
    /// </summary>
    public class CartSummaryDto
    {
        public int CartId { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// Individual cart item
    /// </summary>
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string ItemName { get; set; } = null!; // Product or Package name
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; }
    }
}
