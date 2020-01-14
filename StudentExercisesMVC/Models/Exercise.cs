using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExercisesMVC.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Exercise Title is required")]
        [StringLength(99, MinimumLength = 1, ErrorMessage = "Exercise Title length should be between 1 and 99 characters")]
        public string Name { get; set; }
        public string Language { get; set; }
    }
}
