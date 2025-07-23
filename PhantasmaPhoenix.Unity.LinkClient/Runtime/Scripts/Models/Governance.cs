using LunarLabs.Parser;

namespace Phantasma.SDK
{
    public struct Governance
    {
        public string name;
        public string value;
        
        public static Governance FromNode(DataNode node)
        {
            Governance result;

            result.name = node.GetString("name");
            result.value = node.GetString("value");

            return result;
        }
    }
}