using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Newtonsoft.Json.Linq;
using AllWeatherDayAndNightHunters2.Models;

namespace AllWeatherDayAndNightHunters2.Controllers
{
    public class GameHubController: Controller
    {

        private PlayerDb db = new PlayerDb();

        public ActionResult PlayerView()
        {
            var sl = new List<SelectListItem>();

            db.player.ToList().ForEach(item => sl.Add(new SelectListItem() { Text = item.PlayerName, Value = item.PlayerID.ToString() }));

            ViewBag.playerList = sl;
            return View(db.player.ToList());
        }

        public ActionResult PlayerCreate()
        {
            return View(new PlayerViewModel());
        }

        [HttpPost]
        public ActionResult PlayerCreate(AddPlayerBindingModels ap)
        {
            if(ModelState.IsValid)
            {
                PlayerModel p = new PlayerModel
                {
                    PlayerName = ap.Name,
                    GamesPlayed = ap.Games,
                    GamesWon = ap.Won
                };
                db.player.Add(p);
                db.SaveChanges();
                return RedirectToAction("GameView", new { playerId = p.PlayerID });
            }
            return View(ap);
        }

        [HttpPost,ActionName("PlayerView")]
        public ActionResult PlayerPicked(string playerList)
        {
            return RedirectToAction("GameView", new { playerId = playerList });
        }

        public ActionResult GameView(string selectedCountry = null, 
            string weatherIconUrl = null, 
            string coinColor = null, 
            string backgroundColor = null, 
            string playerId = null)
        {
            var sl = new List<SelectListItem>();

            if (playerId != null)
            {
                var p = from player in db.player where player.PlayerID.ToString() == playerId select player;
                ViewBag.ActivPlayer = p.First().PlayerName;
                ViewBag.ActivPlayerId = p.First().PlayerID.ToString();
            }


            string url = (@"https://restcountries.eu/rest/v1/all");
            WebClient client = new WebClient();
            string jsonstring = client.DownloadString(url);

            var t = JsonConvert.DeserializeObject<List<CountryItem>>(jsonstring);

            t.ForEach(item => 
            {
                sl.Add(new SelectListItem()
                {
                    Text = item.name + " [ " + item.capital + " ]",
                    Value = item.capital
                });
                if (selectedCountry != null)
                {
                    if(sl.Last().Value == selectedCountry)
                    {
                        sl.Last().Selected = true;
                    }
                }
            });

            ViewBag.CountryList = sl;

            ViewBag.WeatherIconUrl = weatherIconUrl;

            ViewBag.CoinColor = coinColor;

            ViewBag.BackgroundColor = backgroundColor;

            if (selectedCountry != null)
            {
                return View();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Selection(string CountryList,string pID)
        {
            //Lägg till din kod är..
            //Listan med huvudstad..

            if(CountryList != null)
            {
                string url = string.Format(@"http://api.openweathermap.org/data/2.5/weather?q={0}&APPID=0efffa3566c0a86b9a03fb679a5bab08",CountryList);
                WebClient client = new WebClient();

                string jsonstring = client.DownloadString(url);
                var obj = JObject.Parse(jsonstring);

                //Weather icon representing weather at chosen city, to be used in backgrond image
                string weatherIcon = string.Format(@"http://openweathermap.org/img/w/{0}.png",(string)obj["weather"][0]["icon"]);

                //Longitude and latitude of chosen city
                string lon = (string)obj["coord"]["lon"];
                string lat = (string)obj["coord"]["lat"];

                string timeStamp = DateTime.Now.ToString();
                TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                double currentTimeDouble = span.TotalSeconds;
                currentTimeDouble = Math.Round(currentTimeDouble);

                string currentTimeString = currentTimeDouble.ToString();


                string url2 = string.Format(@"https://maps.googleapis.com/maps/api/timezone/json?location=" + lat + "," + lon + "&timestamp=" + currentTimeString + "& key=AIzaSyAbWTrXF9-X76XxEZH2SsFFNLtdb2ojtAU");


                WebClient client2 = new WebClient();

                string jsonstring2 = client.DownloadString(url2);
                var obj2 = JObject.Parse(jsonstring2);

                //Time difference in seconds of chosen place compared to GMT(?) time
                //To be used to calculate if night or day
                string timeDifference = (string)obj2["rawOffset"];
                int timeDifferenceIntHours = Convert.ToInt32(timeDifference) / 3600;

                string colorOfCoin;
                string colorOfBackground;
                if ((DateTime.UtcNow.Hour + (timeDifferenceIntHours)) > 6 & (DateTime.UtcNow.Hour + (timeDifferenceIntHours)) < 18)
                {
                    colorOfCoin = "#FFD700";
                    colorOfBackground = "#2875e1";
                }
                else
                {
                    colorOfCoin = "#FFFBFF";
                    colorOfBackground = "#000000";
                }


                return RedirectToAction("GameView", new {
                    weatherIconUrl = weatherIcon,
                    coinColor = colorOfCoin,
                    backgroundColor = colorOfBackground,
                    playerId = pID,
                    selectedCountry = CountryList
                });
            }


            return RedirectToAction("GameView");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class CountryItem
    {
        public string name { get; set; }
        public string capital { get; set; }
    }

    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>(() => new Broadcaster());
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan BroadcastInterval =
            TimeSpan.FromMilliseconds(50);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private ShapeModel _model;
        private ShapeModel _coinModel;
        private bool _modelUpdated;
        private bool _coinUpdated;
        private bool _userPick;
        private bool _getUser;
        private string _user;
        public Broadcaster()
        {
            // Save our hub context so we can easily use it 
            // Faskdklas
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();
            _model = new ShapeModel();
            _modelUpdated = false;
            // Start the broadcast loop
            _broadcastLoop = new Timer(
                BroadcastShape,
                null,
                BroadcastInterval,
                BroadcastInterval);
        }
        public void BroadcastShape(object state)
        {
            // No need to send anything if our model hasn't changed
            if (_modelUpdated)
            {
                // This is how we can access the Clients property 
                // in a static hub method or outside of the hub entirely
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateShape(_model);
                _modelUpdated = false;
            }

            //If coin was updated
            if (_coinUpdated)
            {
                _hubContext.Clients.All.updateCoinShape(_coinModel);
                _coinUpdated = false;
            }
            if (_userPick)
            {
                _hubContext.Clients.All.userChoose(_model);
                _userPick = false;
            }
            if (_getUser)
            {
                _hubContext.Clients.Client(_user).getUser(_user);
                _getUser = false;
            }
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
        }
        public void UpdateCoinShape(ShapeModel coinModel)
        {
            _coinModel = coinModel;
            _coinUpdated = true;
        }

        public void UserChoose(ShapeModel clientModel)
        {
            _model = clientModel;
            _userPick = true;
            //_hubContext.Clients.AllExcept(_model.LastUpdatedBy).userChoose(_model);
            //_hubContext.Clients.All.userChoose(_model);
        }

        public void GetUser(string user)
        {
            _getUser = true;
            _user = user;
            //_hubContext.Clients.Client(user).getUser(user);
        }


        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }

    public class MoveShapeHub : Hub
    {
        // Is set via the constructor on each creation
        private Broadcaster _broadcaster;
        private static readonly ConcurrentDictionary<string, ShapeModel> _connections =
            new ConcurrentDictionary<string, ShapeModel>();

        public MoveShapeHub()
            : this(Broadcaster.Instance)
        {
        }
        public MoveShapeHub(Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }
        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;
            // Update the shape model within our broadcaster
            _broadcaster.UpdateShape(clientModel);
        }

        public Task UpdateScore(ShapeModel clientModel)
        {
            if (System.Web.HttpContext.Current.Application["score"] == null)
            {
                System.Web.HttpContext.Current.Application["score"] = 0;
            }
            else
            {
                System.Web.HttpContext.Current.Application["score"] = Convert.ToInt32(System.Web.HttpContext.Current.Application["score"]) + 1;
            }

            int testScore = Convert.ToInt32(System.Web.HttpContext.Current.Application["score"]);

            ShapeModel sm = new ShapeModel();
            _connections.TryGetValue(Context.ConnectionId,out sm);

            sm.CoinScore += 5;

            foreach (var item in _connections.Values)
            {
                if(item.CoinScore > 100)
                {
                    var list = _connections.Values.ToList();
                    var t = new Task( async () =>
                    {
                        await updateDb(list, item);
                    });
                    t.Start();
                    return Clients.All.winner(item);
                }
            }
            
            return Clients.All.updateScore(sm);
            // Update the shape model within our broadcaster
        }

        public async Task<int> updateDb(List<ShapeModel> shapeList,ShapeModel winner)
        {
            using (var db = new PlayerDb())
            {
                var players = from p in db.player select p;

                foreach (var p in players)
                {
                    if(shapeList.Exists(s => s.ActivePlayerId == p.PlayerID))
                    {
                        p.GamesPlayed += 1;
                    }
                    if (p.PlayerID == winner.ActivePlayerId)
                    {
                        p.GamesWon += 1;
                    }
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                }
                var i = await db.SaveChangesAsync();
                return i;
            }
        }

        public Task ClearScoreForAllUsers()
        {
            foreach (var item in _connections.Values)
            {
                item.CoinScore = 0;
            }
            return Clients.All.clearScoreForAllUsers();
        }

        public void MoveCoin(ShapeModel coinModel)
        {
            //Move coin to random position within game area
            Random rnd = new Random();
            int randLeft = rnd.Next(1, 800);
            int randTop = rnd.Next(1, 600);
            coinModel.Left = randLeft;
            coinModel.Top = randTop;

            // Update the shape model within our broadcaster
            _broadcaster.UpdateCoinShape(coinModel);
        }

        public void GetUser()
        {            
            _broadcaster.GetUser(Context.ConnectionId);
        }

        public async Task<string> GetUserN()
        {
           return await Clients.Caller.getUserN(Context.ConnectionId);
        }

        public void SetUserName(ShapeModel sm)
        {
            ShapeModel model = new ShapeModel();
            _connections.TryGetValue(sm.ShapeOwner, out model);
            model.ShapeId = sm.ShapeId;
            model.ActivePlayerId = sm.ActivePlayerId;
            ShapeModel temp = new ShapeModel();
            _connections.TryRemove(sm.ShapeOwner, out temp);
            _connections.TryAdd(model.ShapeOwner, model);
        }


        public void UserChoose(ShapeModel clientModel)
        {
            if (clientModel.ShapeOwner == null ||
                clientModel.ShapeOwner.Equals("none"))
            {
                clientModel.LastUpdatedBy = Context.ConnectionId;
                clientModel.ShapeOwner = Context.ConnectionId;
                _broadcaster.UserChoose(clientModel);
            }
        }

        public async Task<ShapeModel> OtherPlayer(ShapeModel clientModel)
        {
            clientModel.ShapeOwner = Context.ConnectionId;
            ShapeModel temp = new ShapeModel();
            _connections.TryGetValue(clientModel.ShapeOwner, out temp);
            return await Clients.Others.otherPlayer(temp);
        }

        public async Task<int> SaveScore(ShapeModel winnerShape)
        {
            using (var db = new PlayerDb())
            {
                var players = from p in db.player select p;

                foreach(var p in players)
                {
                    p.GamesPlayed += 1;
                    if(p.PlayerID == winnerShape.ActivePlayerId)
                    {
                        p.GamesWon += 1;
                    }
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                }
                var i = await db.SaveChangesAsync();
                return i;
            }
        }

        public override Task OnConnected()
        {

            int free = 0;
            ShapeModel sm = new ShapeModel();
            sm.ShapeOwner = Context.ConnectionId;
            //sm.PlayerId = "player" + (_connections.Count + 1).ToString();
            _connections.ReturnFreeId(out free);
            sm.PlayerId = "player" + free.ToString();
            sm.ShapeId = free.ToString();
            _connections.TryAdd(Context.ConnectionId, sm);
            return Clients.Caller.clientConnected(sm);
        }

        //public override Task OnConnected()
        //{
        //    _connections.TryAdd(Context.ConnectionId, null);
        //    return Clients.All.clientCountChanged(_connections.Count);
        //}

        public override Task OnReconnected()
        {
            _connections.TryAdd(Context.ConnectionId, null);
            return Clients.All.clientCountChanged(_connections.Count);
        }

        public override Task OnDisconnected(bool stopcalled)
        {
            ShapeModel sm = new ShapeModel();
            sm.ShapeOwner = Context.ConnectionId;
            sm.PlayerId = "player" + _connections.Count.ToString();
            sm.CoinScore = 0;
            ShapeModel value;
            _connections.TryRemove(Context.ConnectionId, out value);
            sm.PlayerId = value.PlayerId;
            return Clients.AllExcept(Context.ConnectionId).clientDisconnected(sm);
        }


        //public override Task OnDisconnected()
        //{
        //    object value;
        //    _connections.TryRemove(Context.ConnectionId, out value);
        //    return Clients.All.clientCountChanged(_connections.Count);
        //}
    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
        [JsonProperty("ShapeId")]
        public string ShapeId { get; set; }
        [JsonProperty("ShapeOwner")]
        public string ShapeOwner { get; set; }
        [JsonProperty("PlayerId")]
        public string PlayerId { get; set; }
        [JsonProperty("CoinScore")]
        public int CoinScore { get; set; }
        [JsonProperty("ActivePlayerId")]
        public int ActivePlayerId { get; set; }
    }
    
}