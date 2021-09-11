using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ColmanGym.Models
{
    public class Training
    {
        [Key]
        [ScaffoldColumn(false)]
        public int TrainingId { get; set; }

        [Required]
        [Display(Name = "Training Name")]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; }

        [Display(Name = "Training Target")]
        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Target { get; set; }

        public ICollection<Meeting> Meetings { get; set; }

    }
}
