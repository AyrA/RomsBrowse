using RomsBrowse.Common;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class SearchViewModel : IValidateable
    {
        [Required]
        public string? Search { get; set; }
        public int? Platform { get; set; }

#pragma warning disable CS8774
        [MemberNotNull(nameof(Search))]
        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
#pragma warning restore CS8774
    }
}
