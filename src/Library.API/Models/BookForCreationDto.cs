using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForCreationDto
    {
        [Required(ErrorMessage = "You should pass the title")]
        [MaxLength(100, ErrorMessage ="The title can not be more than 100 charactres")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "The description can not be more than 500 charactres")]
        public string Description { get; set; }
    }
}