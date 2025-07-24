namespace Phantasma.SDK
{
    public class Token
    {
        public string symbol; //
        public string name; //
        public int decimals; //
        public string currentSupply; //
        public string maxSupply; //
        public string burnedSupply; //
        public string address; //
        public string owner; //
        public string flags; //
        public string script; //
        public TokenExternal[] external;
        public TokenSeries[] series;
    }
}