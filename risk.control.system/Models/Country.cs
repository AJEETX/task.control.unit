
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models;
public class Country : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string CountryId { get; set; } = Guid.NewGuid().ToString();
    [Display(Name = "Country name")]
    public string Name { get; set; } = default!;
    [Display(Name = "Country code")]
    [Required]
    public string Code { get; set; } = default!;

}
