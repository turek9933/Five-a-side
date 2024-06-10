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
        private readonly string file_all_players_name = "./Data/players.json";
        private readonly string file_local_players_name = "./Data/local_players.json";
        private readonly string file_temp_player_name = "./Data/temp_player.json";
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

        private Player JsonSearchToPlayer(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                JsonElement resultsElement = root.GetProperty("results");

                if (resultsElement.GetArrayLength() > 0)
                {
                    JsonElement firstPlayerElement = resultsElement[0];
                    return new Player(
                        firstPlayerElement.GetProperty("id").GetString(),
                        firstPlayerElement.GetProperty("name").GetString(),
                        firstPlayerElement.GetProperty("age").GetString() == "-" ? 0 : int.Parse(firstPlayerElement.GetProperty("age").GetString()),
                        firstPlayerElement.GetProperty("nationalities")[0].GetString(),
                        firstPlayerElement.GetProperty("club").GetProperty("name").GetString() == "---" ? "Retired" : firstPlayerElement.GetProperty("club").GetProperty("name").GetString(),
                        firstPlayerElement.GetProperty("marketValue").GetString() == "-" ? "€0" : firstPlayerElement.GetProperty("marketValue").GetString()
                    );
                }
            }
            return null;
        }

        private Player JsonIdToPlayer(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                var club = root.GetProperty("club").GetProperty("name").GetString() == "---" ? "Retired" : root.GetProperty("club").GetProperty("name").GetString();
                return new Player(
                    root.GetProperty("id").GetString(),
                    root.GetProperty("name").GetString(),
                    root.GetProperty("age").GetString() == "-" ? 0 : int.Parse(root.GetProperty("age").GetString()),
                    root.GetProperty("citizenship")[0].GetString(),
                    club,
                    club == "Retired" ? "€0" : (root.GetProperty("marketValue").GetString() == "-" ? "€0" : root.GetProperty("marketValue").GetString())
                );
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
                    return foundPlayer != null && foundPlayer.Equals(player);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new PlayersControllerException($"Error during getting player by ID from transfermarkt API: {ex.Message}");
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
            catch (Exception ex)
            {
                throw new PlayerException($"Error during searching for player in transfermarkt API: {ex.Message}");
            }
        }

        [HttpGet("temp-player")]
        public IActionResult GetTempPlayer()
        {
            Player tempPlayer = Player.LoadPlayerFromFile(file_temp_player_name);
            if (tempPlayer == null)
            {
                return NotFound("No temporary player found.");
            }
            return Ok(tempPlayer);
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
            Player tempPlayer = Player.LoadPlayerFromFile(file_temp_player_name);
            if (tempPlayer == null)
            {
                return BadRequest("No player loaded to save. Put player first!");
            }

            List<Player> players = Player.LoadPlayersFromFile(file_local_players_name);

            if (!players.Any(p => p.Id == tempPlayer.Id))
            {
                players.Add(tempPlayer);
                Player.SavePlayersToFile(players, file_local_players_name);
                return Ok($"Player:\n\n{tempPlayer}\n\nsaved successfully.");
            }
            else
            {
                return Ok($"Player:\n\n{tempPlayer}\n\nalready saved.");
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
            catch (Exception ex)
            {
                throw new PlayerException($"Error during getting player by ID from transfermarkt API: {ex.Message}");
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
            Player.SavePlayersToFile(updatedPlayers, file_local_players_name);

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
            Player.SavePlayersToFile(updatedPlayers, file_local_players_name);

            return Ok($"Player with ID {player_id} deleted successfully.");
        }
    }
}
