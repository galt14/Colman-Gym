using ColmanGym.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmanGym.Models
{
    public class Meeting : IValidatableObject
    {
        [Key]
        public int MeetID { get; set; }

        [ForeignKey("TrainingID")]
        public int TrainingID { get; set; }

        [ForeignKey("UserId")]
        public string TrainerID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Price { get; set; }

        public ApplicationUser Trainer { get; set; }
        public Training Training { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            if (Date < DateTime.Now)
            {
                results.Add(new ValidationResult("Can not create meeting with past date", new[] { "Date" }));
            }

            return results;
        }

    }
}