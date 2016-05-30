namespace AllWeatherDayAndNightHunters2.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<AllWeatherDayAndNightHunters2.Models.PlayerDb>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(AllWeatherDayAndNightHunters2.Models.PlayerDb context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.player.AddOrUpdate(
                p => new { p.PlayerID, p.PlayerName, p.GamesPlayed, p.GamesWon },
                new AllWeatherDayAndNightHunters2.Models.PlayerModel
                {
                    PlayerID = 1,
                    PlayerName = "Lempa",
                    GamesPlayed = 100000,
                    GamesWon = 99998
                }
                );
        }
    }
}
