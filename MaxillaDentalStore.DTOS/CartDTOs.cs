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
    }

    public class UpdateCartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}