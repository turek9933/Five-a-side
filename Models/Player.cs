using System.Text.Json;
using System.Text.RegularExpressions;
using Five_a_side.Exceptions;

namespace Five_a_side.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Nation { get; set; }
        public string Club { get; set; }
        public string Value { get; set; }

        public Player(string id, string name, int age, string nation, string club, string value)
        {
            string fixedValue = fixValue(value);

            if (rightValue(fixedValue))
            {
                Id = id;
                Name = name;
                Age = age;
                Nation = nation;
                Club = club;
                Value = fixedValue;
            }
            else
            {
                throw new PlayerException("Player Constructor Error");
            }
        }

        public override string ToString()
        {
            return "ID: " + Id +
                "\nName: " + Name +
                "\nAge: " + Age +
                "\nNation: " + Nation +
                "\nClub: " + Club +
                "\nValue: " + Value;
        }

        private string? fixValue(string value)
        {
            if (value == "-") return "€0";
            else return value;
        }

        private bool rightValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string valuePattern = @"^€\d+(\.\d+)?(m|k)?$";

            return Regex.IsMatch(value, valuePattern);
        }

        public string changedValue(string value, string currenceCode, float exchangeRate)
        {
            if (rightValue(value))
            {
                Match match = Regex.Match(value, @"(?<=€)\d+(\.\d+)?");
                float valueFloat;
                if (match.Success && float.TryParse(match.Value, out valueFloat))
                {
                    // Określenie mnożnika i przeliczenie wartości
                    int multiplie = value.EndsWith("m") ? 1000000 : value.EndsWith("k") ? 1000 : 1;
                    valueFloat *= exchangeRate * multiplie;

                    if (valueFloat >= 1000000)
                    {
                        return (valueFloat / 1000000).ToString("F2") + "m " + currenceCode;
                    }
                    else if (valueFloat >= 1000)
                    {
                        return (valueFloat / 1000).ToString("F2") + "k " + currenceCode;
                    }
                    else
                    {
                        return valueFloat.ToString("F2") + " " + currenceCode;
                    }
                }
                else
                {
                    throw new PlayerException("Error on loading float number from player value");
                }
            }
            else
            {
                throw new PlayerException("Player value does not match the pattern");
            }
        }

        public static List<Player> LoadPlayersFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<Player>>(jsonString) ?? new List<Player>();
                }
                catch (Exception)
                {
                    throw new PlayerException("Blad pobierania danych piłkarzy z pliku.");
                }
            }
            else
            {
                return new List<Player>();
            }
        }

        public static Player LoadPlayerFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<Player>(jsonString);
                }
                catch (Exception)
                {
                    throw new PlayerException("Blad pobierania danych piłkarza z pliku.");
                }
            }
            else
            {
                return null;
            }
        }

        public static void SavePlayerToFile(Player data, string filePath)
        {
            string jsonData = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public static void SavePlayersToFile(List<Player> data, string filePath)
        {
            string jsonData = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Player)obj;
            return Id == other.Id &&
                   Name == other.Name &&
                   Age == other.Age &&
                   Nation == other.Nation &&
                   Club == other.Club &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Age, Nation, Club, Value);
        }
    }
}
