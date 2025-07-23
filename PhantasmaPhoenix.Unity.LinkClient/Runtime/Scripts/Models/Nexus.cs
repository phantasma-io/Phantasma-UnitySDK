using LunarLabs.Parser;
using Phantasma.Business.Blockchain.Contracts.Native;

namespace Phantasma.SDK
{
    public struct Nexus
    {
        public string name; //
        public uint protocol; //
        public Platform[] platforms; //
        public Token[] tokens;
        public Chain[] chains; //
        public Governance[] governance; //
        public string[] organizations; //
        
        public static Nexus FromNode(DataNode node)
        {
            Nexus result;

            result.name = node.GetString("name");
            result.protocol = node.GetUInt32("protocol");
            
            var platforms_array = node.GetNode("platforms");
            if (platforms_array != null)
            {
                result.platforms = new Platform[platforms_array.ChildCount];
                for (int i = 0; i < platforms_array.ChildCount; i++)
                {

                    result.platforms[i] = Platform.FromNode(platforms_array.GetNodeByIndex(i));
                }
            }
            else
            {
                result.platforms = new Platform[0];
            }
            
            var tokens_array = node.GetNode("tokens");
            if (tokens_array != null)
            {
                result.tokens = new Token[tokens_array.ChildCount];
                for (int i = 0; i < tokens_array.ChildCount; i++)
                {

                    result.tokens[i] = Token.FromNode(tokens_array.GetNodeByIndex(i));
                }
            }
            else
            {
                result.tokens = new Token[0];
            }
            
            var chains_array = node.GetNode("chains");
            if (chains_array != null)
            {
                result.chains = new Chain[chains_array.ChildCount];
                for (int i = 0; i < chains_array.ChildCount; i++)
                {

                    result.chains[i] = Chain.FromNode(chains_array.GetNodeByIndex(i));
                }
            }
            else
            {
                result.chains = new Chain[0];
            }
            
            var governance_array = node.GetNode("governance");
            if (governance_array != null)
            {
                result.governance = new Governance[governance_array.ChildCount];
                for (int i = 0; i < governance_array.ChildCount; i++)
                {

                    result.governance[i] = Governance.FromNode(governance_array.GetNodeByIndex(i));
                }
            }
            else
            {
                result.governance = new Governance[0];
            }
            
            var organizations_array = node.GetNode("organizations");
            if (organizations_array != null)
            {
                result.organizations = new string[organizations_array.ChildCount];
                for (int i = 0; i < organizations_array.ChildCount; i++)
                {

                    result.organizations[i] = organizations_array.GetNodeByIndex(i).AsString();
                }
            }
            else
            {
                result.organizations = new string[0];
            }
            return result;
        }
    }
}