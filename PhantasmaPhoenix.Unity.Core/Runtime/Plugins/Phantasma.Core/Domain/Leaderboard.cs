using System.Numerics;
using LunarLabs.Parser;
using Phantasma.Core.Cryptography;

namespace Phantasma.Core.Domain
{
    public struct LeaderboardRow
    {
        public Address address;
        public BigInteger value;

        public static LeaderboardRow FromNode(DataNode node)
        {
            var row = new LeaderboardRow();
            row.address = Address.FromText(node.GetString("address"));
            row.value = BigInteger.Parse(node.GetString("value"));
            return row;
        }
    }

    public struct Leaderboard
    {
        public string name;
        public LeaderboardRow[] rows;
        
        public static Leaderboard FromNode(DataNode node)
        {
            var leaderboard = new Leaderboard();
            leaderboard.name = node.GetString("name");
            var rows = node.GetNode("rows");
            var list = new System.Collections.Generic.List<LeaderboardRow>();
            foreach (var row in rows.Children)
            {
                list.Add(LeaderboardRow.FromNode(row));
            }
            leaderboard.rows = list.ToArray();
            return leaderboard;
        }
    }

}
