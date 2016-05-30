using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AllWeatherDayAndNightHunters2.Models
{
    public class AddPlayerBindingModels
    {
        public AddPlayerBindingModels()
        {
        }
        [Required]
        [Display(Name = "Player name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Games played")]
        public int Games { get; set; }
        [Required]
        [Display(Name = "Games won")]
        public int Won { get; set; }
    }

}