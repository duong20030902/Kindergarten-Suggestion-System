﻿using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Areas.Administration.ViewModels
{
    public class FacilityVm
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
