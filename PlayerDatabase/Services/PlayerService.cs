using PlayerDatabase.Models;
using System.Collections.Generic;

namespace PlayerDatabase.Services
{
	public static class PlayerService
	{
		static List<Player> Players { get; }
		static int nextId;

		static PlayerService()
		{
			// TODO : Deserialize JSON from local files
			Players = new List<Player>
		{
			new Player { Id = 1, Name = "Rio", Win = 1, Lose = 1 }
		};

			nextId = Players.Count + 1;
		}

		public static List<Player> GetAll() => Players;

		public static Player? Get(int id) => Players.FirstOrDefault(p => p.Id == id);

		public static Player? Get(string name) => Players.FirstOrDefault(p => p.Name == name);

		public static void Add(Player player)
		{
			player.Id = nextId++;
			Players.Add(player);
		}

		public static void Update(Player player)
		{
			var index = Players.FindIndex(p => p.Id == player.Id);
			if (index == -1)
				return;

			Players[index] = player;
		}

		public static void Delete(int id)
		{
			var player = Get(id);
			if (player is null)
				return;

			Players.Remove(player);
			nextId--;
		}
	}
}
