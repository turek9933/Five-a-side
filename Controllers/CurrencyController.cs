using Microsoft.AspNetCore.Mvc;
using Five_a_side.Exceptions;
using Five_a_side.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace Five_a_side.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly string file_all_players_name = "./Data/players.json";
        private readonly string file_local_players_name = "./Data/local_players.json";
        private readonly string file_temp_player_name = "./Data/temp_player.json";
        private readonly string file_local_teams_name = "./Data/local_teams.json";
        private readonly string file_temp_team_name = "./Data/temp_team.json";
        private readonly IHttpClientFactory _clientFactory;

        public CurrencyController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private decimal valueInEuro(string value)
        {
            decimal multiplier = 1;

            if (value.EndsWith("m"))
            {
                multiplier = 1000000;
            }
            else if (value.EndsWith("k"))
            {
                multiplier = 1000;
            }

            string num = value.Substring(1).TrimEnd('m', 'k');

            try
            {
                return decimal.Parse(num) * multiplier;
            }
            catch
            {
                throw new CurrencyControllerException("Unable to extract euro value from the given string.");
            }
        }

        private string valueWithSuffix(decimal value)
        {
            string suffix = "";
            ulong resultValue = (ulong)value;

            try
            {
                if (value >= 1000000)
                {
                    suffix = "m";
                    resultValue = (ulong)(value / 1000000);
                }
                else if (value >= 1000)
                {
                    suffix = "k";
                    resultValue = (ulong)(value / 1000);
                }
                return $"{resultValue}{suffix}";
            }
            catch
            {
                throw new CurrencyControllerException("Unable to calculate value suffix");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencyRates()
        {
            var client = _clientFactory.CreateClient("NbpClient");

            try
            {
                var response = await client.GetAsync("/api/exchangerates/tables/a/");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    using (var document = JsonDocument.Parse(jsonString))
                    {
                        var root = document.RootElement;
                        var ratesElement = root[0].GetProperty("rates");

                        var rates = JsonSerializer.Deserialize<List<CurrencyRate>>(ratesElement.GetRawText());

                        return Ok(rates);
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new CurrencyControllerException($"Error during searching for Currencies in NBP API: {ex.Message}");
            }
        }

        [HttpGet("temp-player-in-currency/{targetCurrencyCode}")]
        public async Task<IActionResult> ConvertTempPlayerValue(string targetCurrencyCode)
        {
            Player tempPlayer = Player.LoadPlayerFromFile(file_temp_player_name);
            if (tempPlayer == null)
            {
                return BadRequest("No temporary player found.");
            }

            var client = _clientFactory.CreateClient("NbpClient");
            var response = await client.GetAsync("/api/exchangerates/tables/a/");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using (var document = JsonDocument.Parse(jsonString))
            {
                var root = document.RootElement;
                var ratesElement = root[0].GetProperty("rates");

                var rates = JsonSerializer.Deserialize<List<CurrencyRate>>(ratesElement.GetRawText());

                CurrencyRate targetRate = targetCurrencyCode == "PLN" ? new CurrencyRate("Polski złoty", "PLN", 1) : rates.FirstOrDefault(r => r.Code == targetCurrencyCode);

                if (targetRate == null)
                {
                    return NotFound($"Currency code {targetCurrencyCode} not found.");
                }

                var euroRate = rates.FirstOrDefault(r => r.Code == "EUR");
                if (euroRate == null)
                {
                    return NotFound($"Euro rate has not been found.");
                }

                var euroValue = valueInEuro(tempPlayer.Value);
                var convertedValue = euroValue * euroRate.Mid / targetRate.Mid;

                return Ok(new
                {
                    Player = tempPlayer.Name,
                    OriginalValue = tempPlayer.Value,
                    ConvertedValue = $"{valueWithSuffix(convertedValue)} {targetCurrencyCode}"
                });
            }
        }

        [HttpGet("player-in-currency/{playerId}/{targetCurrencyCode}")]
        public async Task<IActionResult> ConvertPlayerValue(string playerId, string targetCurrencyCode)
        {
            var players = Player.LoadPlayersFromFile(file_local_players_name);

            var player = players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
            {
                return NotFound($"Player with ID {playerId} not found.");
            }

            var client = _clientFactory.CreateClient("NbpClient");
            var response = await client.GetAsync("/api/exchangerates/tables/a/");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using (var document = JsonDocument.Parse(jsonString))
            {
                var root = document.RootElement;
                var ratesElement = root[0].GetProperty("rates");

                var rates = JsonSerializer.Deserialize<List<CurrencyRate>>(ratesElement.GetRawText());

                CurrencyRate targetRate = targetCurrencyCode == "PLN" ? new CurrencyRate("Polski złoty", "PLN", 1) : rates.FirstOrDefault(r => r.Code == targetCurrencyCode);

                if (targetRate == null)
                {
                    return NotFound($"Currency code {targetCurrencyCode} not found.");
                }

                var euroRate = rates.FirstOrDefault(r => r.Code == "EUR");
                if (euroRate == null)
                {
                    return NotFound($"Euro rate has not been found.");
                }

                var euroValue = valueInEuro(player.Value);
                var convertedValue = euroValue * euroRate.Mid / targetRate.Mid;

                return Ok(new
                {
                    Player = player.Name,
                    OriginalValue = player.Value,
                    ConvertedValue = $"{valueWithSuffix(convertedValue)} {targetCurrencyCode}"
                });
            }
        }

        [HttpGet("temp-team-in-currency/{targetCurrencyCode}")]
        public async Task<IActionResult> ConvertTempTeamValue(string targetCurrencyCode)
        {
            Team tempTeam = Team.LoadTeamFromFile(file_temp_team_name);
            if (tempTeam == null)
            {
                return BadRequest("No temporary team found.");
            }

            var client = _clientFactory.CreateClient("NbpClient");
            var response = await client.GetAsync("/api/exchangerates/tables/a/");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using (var document = JsonDocument.Parse(jsonString))
            {
                var root = document.RootElement;
                var ratesElement = root[0].GetProperty("rates");

                var rates = JsonSerializer.Deserialize<List<CurrencyRate>>(ratesElement.GetRawText());

                CurrencyRate targetRate = targetCurrencyCode == "PLN" ? new CurrencyRate("Polski złoty", "PLN", 1) : rates.FirstOrDefault(r => r.Code == targetCurrencyCode);

                if (targetRate == null)
                {
                    return NotFound($"Currency code {targetCurrencyCode} not found.");
                }

                var euroRate = rates.FirstOrDefault(r => r.Code == "EUR");
                if (euroRate == null)
                {
                    return NotFound($"Euro rate has not been found.");
                }

                var players = new List<Player> { tempTeam.GK, tempTeam.LB, tempTeam.RB, tempTeam.LF, tempTeam.RF };
                decimal totalValueInEuros = players.Sum(player => valueInEuro(player.Value));
                var convertedValue = totalValueInEuros * euroRate.Mid / targetRate.Mid;

                return Ok(new
                {
                    Team = tempTeam.Name,
                    OriginalValue = $"€{valueWithSuffix(totalValueInEuros)}",
                    ConvertedValue = $"{valueWithSuffix(convertedValue)} {targetCurrencyCode}"
                });
            }
        }

        [Authorize]
        [HttpGet("team-in-currency/{teamId}/{targetCurrencyCode}")]
        public async Task<IActionResult> ConvertTeamValue(string teamId, string targetCurrencyCode)
        {
            var teams = Team.LoadTeamsFromFile(file_local_teams_name);

            var team = teams.FirstOrDefault(t => t.Id.ToString() == teamId);
            if (team == null)
            {
                return NotFound($"Team with ID {teamId} not found.");
            }

            var client = _clientFactory.CreateClient("NbpClient");
            var response = await client.GetAsync("/api/exchangerates/tables/a/");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using (var document = JsonDocument.Parse(jsonString))
            {
                var root = document.RootElement;
                var ratesElement = root[0].GetProperty("rates");

                var rates = JsonSerializer.Deserialize<List<CurrencyRate>>(ratesElement.GetRawText());

                CurrencyRate targetRate = targetCurrencyCode == "PLN" ? new CurrencyRate("Polski złoty", "PLN", 1) : rates.FirstOrDefault(r => r.Code == targetCurrencyCode);

                if (targetRate == null)
                {
                    return NotFound($"Currency code {targetCurrencyCode} not found.");
                }

                var euroRate = rates.FirstOrDefault(r => r.Code == "EUR");
                if (euroRate == null)
                {
                    return NotFound($"Euro rate has not been found.");
                }

                var players = new List<Player> { team.GK, team.LB, team.RB, team.LF, team.RF };
                decimal totalValueInEuros = players.Sum(player => valueInEuro(player.Value));
                var convertedValue = totalValueInEuros * euroRate.Mid / targetRate.Mid;

                return Ok(new
                {
                    Team = team.Name,
                    OriginalValue = $"€{valueWithSuffix(totalValueInEuros)}",
                    ConvertedValue = $"{valueWithSuffix(convertedValue)} {targetCurrencyCode}"
                });
            }
        }
    }
}
