using Microsoft.AspNetCore.Mvc;
using Five_a_side.Exceptions;
using Five_a_side.Models;
using System.Text.Json;

namespace Five_a_side.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        string file_all_players_name = "./Data/players.json";
        string file_local_players_name = "./Data/local_players.json";
        string file_temp_player_name = "./Data/temp_player.json";

        private readonly IHttpClientFactory _clientFactory;

        public PlayersController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            InitializeTempPlayerFile();
        }

        private void InitializeTempPlayerFile()
        {
            if (!System.IO.File.Exists(file_temp_player_name) || new FileInfo(file_temp_player_name).Length == 0)
            {
                Player.SavePlayerToFile(new Player("8198", "Cristiano Ronaldo", 39, "Portugal", "Al-Nassr FC", "€15.00m"), file_temp_player_name);
            }
        }

        private Player findPlayer(List<Player> playerList, string playerId)
        {
            return playerList.FirstOrDefault(playerTemp => playerTemp.Id == playerId);
        }

        private Player JsonSearchToPlayer(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                JsonElement resultsElement = root.GetProperty("results");

                if (resultsElement.GetArrayLength() > 0)
                {
                    JsonElement firstPlayerElement = resultsElement[0];

                    string id = firstPlayerElement.GetProperty("id").GetString();
                    string name = firstPlayerElement.GetProperty("name").GetString();
                    int age = firstPlayerElement.GetProperty("age").GetString() == "-" ? 0 : int.Parse(firstPlayerElement.GetProperty("age").GetString());
                    string nation = firstPlayerElement.GetProperty("nationalities")[0].GetString();
                    string club = firstPlayerElement.GetProperty("club").GetProperty("name").GetString() == "---" ? "Retired" : firstPlayerElement.GetProperty("club").GetProperty("name").GetString();

                    string marketValue = firstPlayerElement.GetProperty("marketValue").GetString();
                    marketValue = marketValue == "-" ? "€0" : marketValue;

                    return new Player(id, name, age, nation, club, marketValue);
                }
            }
            return null;
        }
        private Player JsonIdToPlayer(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;

                string id = root.GetProperty("id").GetString();
                string name = root.GetProperty("name").GetString();
                int age = root.GetProperty("age").GetString() == "-" ? 0 : int.Parse(root.GetProperty("age").GetString());
                string nation = root.GetProperty("citizenship")[0].GetString();
                string club = root.GetProperty("club").GetProperty("name").GetString() == "---" ? "Retired" : root.GetProperty("club").GetProperty("name").GetString();

                string marketValue = "€0";
                if (club != "Retired")
                {
                    marketValue = root.GetProperty("marketValue").GetString();
                    marketValue = marketValue == "-" ? "€0" : marketValue;
                }

                return new Player(id, name, age, nation, club, marketValue);
            }
        }

        private async Task<bool> CheckPlayer(Player player)
        {
            var client = _clientFactory.CreateClient("TransfermarktClient");
            try
            {
                var response = await client.GetAsync($"players/search/{player.Name}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Player foundPlayer = JsonSearchToPlayer(jsonString);

                    Console.WriteLine(foundPlayer);
                    Console.WriteLine(player);

                    return foundPlayer != null && foundPlayer.Equals(player);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw new PlayersControllerException("Error during getting player by ID from transfermarkt API");
            }
        }

        [HttpGet]
        public IEnumerable<Player> Get()
        {
            return Player.LoadPlayersFromFile(file_local_players_name);
        }

        [HttpGet("search-player/{searchTerm}")]
        public async Task<IActionResult> SearchPlayerOnTransfermarkt(string searchTerm)
        {
            var client = _clientFactory.CreateClient("TransfermarktClient");
            try
            {
                var response = await client.GetAsync($"players/search/{searchTerm}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return Ok(JsonSearchToPlayer(jsonString));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw new PlayerException("Error during searching for player in transfermarkt API");
            }
        }


        [HttpGet("temp-player")]
        public Player GetTempPlayer()
        {
            return Player.LoadPlayerFromFile(Path.Combine(Directory.GetCurrentDirectory(), file_temp_player_name));
        }

        [HttpPut("temp-player")]
        public async Task<IActionResult> PutTempPlayer([FromBody] Player player)
        {
            try
            {
                if (await CheckPlayer(player))
                {
                    Player.SavePlayerToFile(player, file_temp_player_name);
                    return Ok($"Player:\n\n{player}\n\nsaved successfully.");
                }
                else
                {
                    return BadRequest("Player has not valid data.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while saving the player: {ex.Message}");
            }
        }

        [HttpPost("save-temp-player")]
        public IActionResult SavePlayer()
        {
            Player temp_player = Player.LoadPlayerFromFile(Path.Combine(Directory.GetCurrentDirectory(), file_temp_player_name));
            if (temp_player == null)
            {
                return BadRequest("No player loaded to save. Put player first!");
            }

            List<Player> players = Player.LoadPlayersFromFile(file_local_players_name);

            if (!players.Any(p => p.Id == tempPlayer.Id))
            {
                players.Add(temp_player);
                Player.SavePlayersToFile(players, Path.Combine(Directory.GetCurrentDirectory(), file_local_players_name));
                return Ok($"Player:\n\n{temp_player}\n\nsaved successfully.");
            }
            else
            {
                return Ok($"Player:\n\n{temp_player}\n\nalready saved.");
            }
        }

        [HttpGet("get-player/{player_id}")]
        public async Task<IActionResult> GetPlayerOnTransfermarkt(int player_id)
        {
            var client = _clientFactory.CreateClient("TransfermarktClient");
            try
            {
                var response = await client.GetAsync($"players/{player_id}/profile");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return Ok(JsonIdToPlayer(jsonString));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw new PlayerException("Error during getting player by ID from transfermarkt API");
            }
        }

        [HttpDelete("delete-player")]
        public IActionResult DeletePlayer([FromBody] Player data)
        {
            List<Player> players = Player.LoadPlayersFromFile(file_local_players_name);
            Player playerToDelete = players.FirstOrDefault(p => p.Equals(data));

            if (playerToDelete == null)
            {
                return NotFound($"Player:\n{data}\nnot found.");
            }

            List<Player> updatedPlayers = players.Where(p => !p.Equals(playerToDelete)).ToList();
            Player.SavePlayersToFile(updatedPlayers, Path.Combine(Directory.GetCurrentDirectory(), file_local_players_name));

            return Ok($"Player:\n{data}\ndeleted successfully.");
        }

        [HttpDelete("delete-player/{player_id}")]
        public IActionResult DeletePlayerById(int player_id)
        {
            List<Player> players = Player.LoadPlayersFromFile(file_local_players_name);
            string playerIdString = player_id.ToString();
            Player playerToDelete = players.FirstOrDefault(p => p.Id == playerIdString);

            if (playerToDelete == null)
            {
                return NotFound($"Player with ID {player_id} not found.");
            }

            List<Player> updatedPlayers = players.Where(p => p.Id != playerIdString).ToList();
            Player.SavePlayersToFile(updatedPlayers, Path.Combine(Directory.GetCurrentDirectory(), file_local_players_name));

            return Ok($"Player with ID {player_id} deleted successfully.");
        }
    }
}
