using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AllWeatherDayAndNightHunters2.Models
{
    public class PlayerModel
    {
        public PlayerModel()
        {

        }
        [Key]
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }
    
    public class PlayerDb : DbContext
    {
        public DbSet<PlayerModel> player { get; set; }
    }
}