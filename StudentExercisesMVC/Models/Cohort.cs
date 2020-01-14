using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExercisesMVC.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Cohort Name is required")]
        [StringLength(99, MinimumLength = 1, ErrorMessage = "Cohort Name length should be between 1 and 99 characters")]
        public string Name { get; set; }
    }
}
