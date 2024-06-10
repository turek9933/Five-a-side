using Microsoft.VisualStudio.TestTools.UnitTesting;
using Five_a_side.Models;

namespace Five_a_side.Tests
{
    [TestClass]
    public class PlayerTests
    {
        [TestMethod]
        public void TestPlayerEquality()
        {
            var player1 = new Player("1", "Player", 25, "Country", "Club", "€1m");
            var player2 = new Player("1", "Player", 25, "Country", "Club", "€1m");

            Assert.AreEqual(player1, player2);
        }
        [TestMethod]
        public void TestPlayerToString()
        {
            var player = new Player("1", "Player One", 25, "Country", "Club", "€1m");
            var expected = "ID: 1\nName: Player One\nAge: 25\nNation: Country\nClub: Club\nValue: €1m";

            var result = player.ToString();

            Assert.AreEqual(expected, result);
        }
    }

    [TestClass]
    public class TeamTests
    {
        [TestMethod]
        public void TestTeamEquality()
        {
            var player1 = new Player("1", "Player One", 25, "Country", "Club", "€1m");
            var player2 = new Player("2", "Player Two", 26, "Country", "Club", "€2m");
            var player3 = new Player("3", "Player Three", 27, "Country", "Club", "€3m");
            var player4 = new Player("4", "Player Four", 28, "Country", "Club", "€4m");
            var player5 = new Player("5", "Player Five", 29, "Country", "Club", "€5m");

            var team1 = new Team
            {
                Id = 1,
                Name = "Team One",
                Description = "First Team",
                GK = player1,
                LB = player2,
                RB = player3,
                LF = player4,
                RF = player5
            };

            var team2 = new Team
            {
                Id = 1,
                Name = "Team One",
                Description = "First Team",
                GK = player1,
                LB = player2,
                RB = player3,
                LF = player4,
                RF = player5
            };

            Assert.AreEqual(team1, team2);
        }

        [TestMethod]
        public void TestTeamInequality()
        {
            var player1 = new Player("1", "Player One", 25, "Country", "Club", "€1m");
            var player2 = new Player("2", "Player Two", 26, "Country", "Club", "€2m");
            var player3 = new Player("3", "Player Three", 27, "Country", "Club", "€3m");
            var player4 = new Player("4", "Player Four", 28, "Country", "Club", "€4m");
            var player5 = new Player("5", "Player Five", 29, "Country", "Club", "€5m");

            var team1 = new Team
            {
                Id = 1,
                Name = "Team One",
                Description = "First Team",
                GK = player1,
                LB = player2,
                RB = player3,
                LF = player4,
                RF = player5
            };

            var team2 = new Team
            {
                Id = 2,
                Name = "Team Two",
                Description = "Second Team",
                GK = player1,
                LB = player2,
                RB = player3,
                LF = player4,
                RF = new Player("6", "Player Six", 30, "Country", "Club", "€6m")
            };

            Assert.AreNotEqual(team1, team2);
        }

        [TestMethod]
        public void TestTeamToString()
        {
            var player1 = new Player("1", "Player One", 25, "Country", "Club", "€1m");
            var player2 = new Player("2", "Player Two", 26, "Country", "Club", "€2m");
            var player3 = new Player("3", "Player Three", 27, "Country", "Club", "€3m");
            var player4 = new Player("4", "Player Four", 28, "Country", "Club", "€4m");
            var player5 = new Player("5", "Player Five", 29, "Country", "Club", "€5m");

            var team = new Team
            {
                Id = 1,
                Name = "Team One",
                Description = "First Team",
                GK = player1,
                LB = player2,
                RB = player3,
                LF = player4,
                RF = player5
            };

            var expected = $"ID: 1\nName: Team One\nDescription: First Team\nGK: {player1}\nLB: {player2}\nRB: {player3}\nLF: {player4}\nRF: {player5}";

            var result = team.ToString();

            Assert.AreEqual(expected, result);
        }
    }

    [TestClass]
    public class CurrencyRateTests
    {

        [TestMethod]
        public void TestCurrencyRateToString()
        {
            var rate = new CurrencyRate("Euro", "EUR", 4.5m);
            var expected = "Euro (EUR): 4.5";

            var result = rate.ToString();

            Assert.AreEqual(expected, result);
        }
    }
}
