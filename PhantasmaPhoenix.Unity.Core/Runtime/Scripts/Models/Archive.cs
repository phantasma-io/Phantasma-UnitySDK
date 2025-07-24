namespace Phantasma.SDK
{
    public struct Archive
    {
        public string name; //
        public string hash; //
        public uint time; //
        public uint size; //
        public string encryption; //
        public int blockCount; //
        public int[] missingBlocks;
        public string[] owners; //
    }
}