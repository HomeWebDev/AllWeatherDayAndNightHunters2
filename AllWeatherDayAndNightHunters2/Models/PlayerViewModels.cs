using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AllWeatherDayAndNightHunters2.Models
{
    public class PlayerViewModel
    {
        public PlayerViewModel()
        {
        }
        [Display(Name = "Player name")]
        public string Name { get; set; }
        [Display(Name = "Games played")]
        public int Games { get; set; }
        [Display(Name = "Games won")]
        public int Won { get; set; }
    }
}