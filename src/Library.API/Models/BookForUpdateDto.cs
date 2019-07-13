using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out the description")]
        public override string Description
        {
            get
            {
                return base.Description;
            }
            set
            {
                base.Description = value;
            }
        }
    }
}