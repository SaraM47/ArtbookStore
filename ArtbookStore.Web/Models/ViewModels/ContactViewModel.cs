using System.ComponentModel.DataAnnotations;

// ViewModel for the contact form. This includes with Name, Email, and Message properties with validation attributes.
namespace ArtbookStore.Web.Models.ViewModels
{
    // This class is used to represent the data for the contact form. It includes validation attributes to ensure that the user provides valid input.
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Message must be at least 10 characters."
        )]
        // The Message property is required and must be between 10 and 1000 characters long.
        public string Message { get; set; } = "";
    }
}
