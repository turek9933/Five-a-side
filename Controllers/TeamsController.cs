using Five_a_side.Exceptions;
using Five_a_side.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Five_a_side.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : Controller
    {
        string file_all_players_name = "./Data/players.json";
        string file_local_teams_name = "./Data/local_teams.json";
        string file_local_players_name = "./Data/local_players.json";

        string file_temp_team_name = "./Data/temp_team.json";

        private readonly IHttpClientFactory _clientFactory;
        public TeamsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            InitializeTempTeamFile();
        }
        private void InitializeTempTeamFile()
        {
            if (!System.IO.File.Exists(file_temp_team_name) || new FileInfo(file_temp_team_name).Length == 0)
            {
                var defaultTeam = new Team
                {
                    Id = 0,
                    Name = "Default Team",
                    Description = "Default temporary team",
                    GK = new Player("59377", "David de Gea", 33, "Spain", "Without Club", "€5.00m"),
                    LB = new Player("117229", "Zbigniew Boniek", 68, "Poland", "Retired", "€0"),
                    RB = new Player("418560", "Erling Haaland", 23, "Norway", "Manchester City", "€180.00m"),
                    LF = new Player("8198", "Cristiano Ronaldo", 39, "Portugal", "Al-Nassr FC", "€15.00m"),
                    RF = new Player("15242", "Grzegorz Rasiak", 45, "Poland", "Retired", "€0")
                };
                Team.SaveTeamToFile(defaultTeam, file_temp_team_name);
            }
        }


        private int GetNextAvailableTeamId(List<Team> teamsList)
        {
            int newId = 0;
            while (teamsList.Any(t => t.Id == newId))
            {
                newId++;
            }
            return newId;
        }

        private bool goodTeamId(Team team, List<Team> teamsList)
        {
            if (team.Id < 0 || teamsList.Any(t => t.Id == team.Id))
            {
                team.Id = GetNextAvailableTeamId(teamsList);
            }

            return !teamsList.Any(t => t.Id == team.Id);
        }

        private bool goodPlayersInTeam(Team team)
        {
            List<Player> players = Player.LoadPlayersFromFile(file_local_players_name);

            bool result = players.Any(p => p.Equals(team.GK)) &&
                          players.Any(p => p.Equals(team.LB)) &&
                          players.Any(p => p.Equals(team.RB)) &&
                          players.Any(p => p.Equals(team.LF)) &&
                          players.Any(p => p.Equals(team.RF));
            return result;
        }

        private async Task<bool> goodGK(Player gk)
        {
            bool result = false;
            var client = _clientFactory.CreateClient("TransfermarktClient");
            try
            {
                var response = await client.GetAsync($"players/{gk.Id}/profile");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        JsonElement root = doc.RootElement;

                        if (root.TryGetProperty("position", out JsonElement positions))
                        {
                            if (positions.TryGetProperty("main", out JsonElement mainPosition))
                            {
                                result = mainPosition.GetString() == "Goalkeeper";
                            }
                            else if (positions.TryGetProperty("other", out JsonElement otherPositions))
                            {
                                foreach (JsonElement otherPosition in otherPositions.EnumerateArray())
                                {
                                    if (otherPosition.GetString() == "Goalkeeper")
                                    {
                                        result = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new TeamsControllerException($"Looking for GK Error. Code: {response.StatusCode} Reason: {response.ReasonPhrase}");
                }
            }
            catch
            {
                throw new TeamsControllerException("Error during checking if GK player is GK player by ID from transfermarkt API");
            }
            return result;
        }

        private async Task<bool> goodTeamData(Team team)
        {
            List<Team> teamsList = Team.LoadTeamsFromFile(file_local_teams_name);

            goodTeamId(team, teamsList);

            if (goodPlayersInTeam(team))
            {
                return await goodGK(team.GK);
            }

            return false;
        }


        [HttpGet("")]
        public IEnumerable<Team> Get()
        {
            return Team.LoadTeamsFromFile(file_local_teams_name);
        }

        [HttpGet("temp-team")]
        public IActionResult GetTempTeam()
        {
            Team tempTeam = Team.LoadTeamFromFile(file_temp_team_name);
            if (tempTeam == null)
            {
                return NotFound("No temporary team found.");
            }
            return Ok(tempTeam);
        }


        [HttpPut("temp-team")]
        public async Task<IActionResult> PutTempTeam([FromBody] Team team)
        {
            List<Team> teamsList = Team.LoadTeamsFromFile(file_local_teams_name);

            goodTeamId(team, teamsList);

            if (await goodTeamData(team))
            {
                Team.SaveTeamToFile(team, file_temp_team_name);
                return Ok("Temporary team saved successfully.");
            }
            else
            {
                return BadRequest("Invalid team data.");
            }
        }




        [HttpPost("save-temp-team")]
        public IActionResult SaveTempTeam()
        {
            Team tempTeam = Team.LoadTeamFromFile(file_temp_team_name);
            if (tempTeam == null)
            {
                return BadRequest("No temporary team to save.");
            }

            List<Team> teams = Team.LoadTeamsFromFile(file_local_teams_name);

            if (!teams.Any(t => t.Id == tempTeam.Id))
            {
                teams.Add(tempTeam);
                Team.SaveTeamsToFile(teams, file_local_teams_name);
                return Ok("Temporary team saved successfully.");
            }
            else
            {
                return Ok("Team with this ID already exists.");
            }
        }


        [HttpGet("get-team/{team_id}")]
        public IActionResult GetTeamById(int team_id)
        {
            List<Team> teams = Team.LoadTeamsFromFile(file_local_teams_name);
            Team team = teams.FirstOrDefault(t => t.Id == team_id);
            if (team == null)
            {
                return NotFound($"Team with ID {team_id} not found.");
            }
            return Ok(team);
        }

        [HttpDelete("delete-team")]
        public IActionResult DeleteTeam([FromBody] Team team)
        {
            List<Team> teams = Team.LoadTeamsFromFile(file_local_teams_name);
            Team teamToDelete = teams.FirstOrDefault(t => t.Equals(team));
            if (teamToDelete == null)
            {
                return NotFound($"Team with ID {team.Id} not found.");
            }

            teams.Remove(teamToDelete);
            Team.SaveTeamsToFile(teams, file_local_teams_name);

            return Ok($"Team with ID {team.Id} deleted successfully.");
        }

        [HttpDelete("delete-team/{team_id}")]
        public IActionResult DeleteTeamById(int team_id)
        {
            List<Team> teams = Team.LoadTeamsFromFile(file_local_teams_name);
            Team teamToDelete = teams.FirstOrDefault(t => t.Id == team_id);
            if (teamToDelete == null)
            {
                return NotFound($"Team with ID {team_id} not found.");
            }

            teams.Remove(teamToDelete);
            Team.SaveTeamsToFile(teams, file_local_teams_name);

            return Ok($"Team with ID {team_id} deleted successfully.");
        }
    }
}
