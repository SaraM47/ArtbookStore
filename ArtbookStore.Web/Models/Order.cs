using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtbookStore.Web.Models;

// Represents a customer order
public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Navigation property to ApplicationUser (ASP.NET Identity user)
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "Pending";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
