namespace Phantasma.SDK
{
    public struct Contract
    {
        public string name; //
        public string address; //
        public string script; //
        public ContractMethod[] methods;
        public ContractEvent[] events;
    }
}