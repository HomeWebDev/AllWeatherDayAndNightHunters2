using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AllWeatherDayAndNightHunters2.Models;

namespace AllWeatherDayAndNightHunters2.Controllers
{
    public class PlayerController : ApiController
    {
        private PlayerDb db = new PlayerDb();

        // GET: api/Player
        public IQueryable<PlayerModel> Getplayer()
        {
            return db.player;
        }

        // GET: api/Player/5
        [ResponseType(typeof(PlayerModel))]
        public IHttpActionResult GetPlayerModel(int id)
        {
            PlayerModel playerModel = db.player.Find(id);
            if (playerModel == null)
            {
                return NotFound();
            }

            return Ok(playerModel);
        }

        // PUT: api/Player/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPlayerModel(int id, PlayerModel playerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != playerModel.PlayerID)
            {
                return BadRequest();
            }

            db.Entry(playerModel).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Player
        [ResponseType(typeof(PlayerModel))]
        public IHttpActionResult PostPlayerModel(PlayerModel playerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.player.Add(playerModel);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = playerModel.PlayerID }, playerModel);
        }

        // DELETE: api/Player/5
        [ResponseType(typeof(PlayerModel))]
        public IHttpActionResult DeletePlayerModel(int id)
        {
            PlayerModel playerModel = db.player.Find(id);
            if (playerModel == null)
            {
                return NotFound();
            }

            db.player.Remove(playerModel);
            db.SaveChanges();

            return Ok(playerModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PlayerModelExists(int id)
        {
            return db.player.Count(e => e.PlayerID == id) > 0;
        }
    }
}