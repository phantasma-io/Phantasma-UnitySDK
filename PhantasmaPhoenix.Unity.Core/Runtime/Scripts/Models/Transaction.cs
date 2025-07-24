using Phantasma.Core.Domain;

namespace Phantasma.SDK
{
    public struct Transaction
    {
        public string hash; //
        public string chainAddress; //
        public uint timestamp; //
        public int blockHeight; //
        public string blockHash; //
        public string script; //
        public string payload;
        public Event[] events; //
        public string result; //
        public string fee; //
        public Signature[] signatures;
        public uint expiration;
        //public int confirmations; //
    }
}