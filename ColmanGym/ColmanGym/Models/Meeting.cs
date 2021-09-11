using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmanGym.Models
{
    public class Meeting : IValidatableObject
    {
        [Key]
        public int MeetId { get; set; }

        [ForeignKey("TrainingId")]
        public int TrainingId { get; set; }

        [ForeignKey("UserId")]
        public string TrainerId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Price { get; set; }

        public User Trainer { get; set; }
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