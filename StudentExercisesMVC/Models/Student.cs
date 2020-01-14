using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExercisesMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "First Name length should be between 1 and 25 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Last Name length should be between 1 and 25 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "SlackHandle is required")]
        [StringLength(99, MinimumLength = 1, ErrorMessage = "SlackHandle length should be between 1 and 99 characters")]
        public string SlackHandle { get; set; }
        
        [Required(ErrorMessage = "Cohort Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Cohort Id should have positive integer value")]
        public int CohortId { get; set; }
        
        [Display(Name = "Cohort")]
        public Cohort Cohort { get; set; }

        public List<Exercise> StudentsExercises = new List<Exercise>();
    }
}
