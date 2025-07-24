namespace Phantasma.SDK
{
    public struct Account
    {
        public string address; //
        public string name; //
        public Stake stakes; //
        public string stake; //
        public string unclaimed;
        public string relay; //
        public string validator; //
        public Balance[] balances; //
        public Storage storage;
        public string[] txs;
    }
}