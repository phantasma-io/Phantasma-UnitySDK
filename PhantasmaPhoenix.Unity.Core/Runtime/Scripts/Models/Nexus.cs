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
    }
}