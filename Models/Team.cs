using Five_a_side.Exceptions;
using System.Text.Json;

namespace Five_a_side.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Player GK { get; set; }
        public Player LB { get; set; }
        public Player RB { get; set; }
        public Player LF { get; set; }
        public Player RF { get; set; }

        public override string ToString()
        {
            return "ID: " + Id +
                   "\nName: " + Name +
                   "\nDescription: " + Description +
                   "\nGK: " + GK +
                   "\nLB: " + LB +
                   "\nRB: " + RB +
                   "\nLF: " + LF +
                   "\nRF: " + RF;
        }

        public static List<Team> LoadTeamsFromFile(string file_local_teams_name)
        {
            string jsonString = System.IO.File.ReadAllText(file_local_teams_name);

            List<Team> teamsList;

            try
            {
                teamsList = JsonSerializer.Deserialize<List<Team>>(jsonString);
            }
            catch (Exception)
            {
                throw new TeamException("Team data import from file error");
            }
            return teamsList;
        }

        public static void SaveTeamToFile(Team data, string file_path)
        {
            string jsonData = JsonSerializer.Serialize(data);

            try
            {
                System.IO.File.WriteAllText(file_path, jsonData);
            }
            catch
            {
                throw new TeamException("Could not save team");
            }
        }

        public static void SaveTeamsToFile(List<Team> teams, string file_path)
        {
            string jsonData = JsonSerializer.Serialize(teams);

            try
            {
                System.IO.File.WriteAllText(file_path, jsonData);
            }
            catch
            {
                throw new TeamException("Could not save teams");
            }
        }

        public static Team LoadTeamFromFile(string file_path)
        {
            if (!System.IO.File.Exists(file_path))
            {
                return null;
            }

            string jsonString = System.IO.File.ReadAllText(file_path);

            Team team;

            try
            {
                team = JsonSerializer.Deserialize<Team>(jsonString);
            }
            catch (Exception)
            {
                throw new TeamException("Team data import from file error");
            }
            return team;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Team)obj;
            return Id == other.Id &&
                   Name == other.Name &&
                   Description == other.Description &&
                   Equals(GK, other.GK) &&
                   Equals(LB, other.LB) &&
                   Equals(RB, other.RB) &&
                   Equals(LF, other.LF) &&
                   Equals(RF, other.RF);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Description, GK, LB, RB, LF, RF);
        }
    }
}
