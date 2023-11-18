namespace AbsGameProject.Blocks;

public static class BlockRegistry
{
    static Dictionary<string, Block> _blocks = new Dictionary<string, Block>();

    public static void AddBlock(Block block)
    {
        if( _blocks.ContainsKey(block.Id) ) { return; }

        _blocks.Add(block.Id, block);
    }

    public static Block? GetBlock(string id)
    {
        if (!_blocks.ContainsKey(id)) { return null; }

        return _blocks[id];
    }
    public static Block? GetBlock(int index)
    {
        return _blocks.ElementAtOrDefault(index - 1).Value;
    }

    public static ushort GetBlockIndex(Block? block)
    {
        if (block == null)
            return 0;

        return (ushort)_blocks.Keys.ToList().IndexOf(block.Id);
    }

    public static List<Block> GetBlocks() 
        => _blocks.Values.ToList();
}
