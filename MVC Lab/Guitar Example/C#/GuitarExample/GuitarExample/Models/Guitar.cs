using System.ComponentModel.DataAnnotations;

namespace GuitarExample.Models
{
    public class Guitar
    {
        [Required]
        public string Name { get; set; }
    }
}