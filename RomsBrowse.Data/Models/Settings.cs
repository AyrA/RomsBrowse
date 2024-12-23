﻿using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomsBrowse.Data.Models
{
#nullable disable
    public class Setting : IValidateable
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None), StringLength(20)]
        public string Name { get; set; }

        public string Value { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
