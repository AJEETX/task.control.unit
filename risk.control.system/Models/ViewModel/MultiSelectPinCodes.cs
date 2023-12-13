using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models.ViewModel
{
    public class MultiSelectPinCodes
    {

        [Required]
        [Display(Name = "Choose Multiple Pincodes")]
        public List<string> SelectedMultiPincodeId { get; set; } = default!;

        /// <summary>  
        /// Gets or sets selected pincodes property.  
        /// </summary>  
        public List<PinCode> SelectedPincodes { get; set; } = default!;

    }
}
