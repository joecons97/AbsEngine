namespace AbsGameProject.Blocks;

public static class BlockRegistry
{
    static Dictionary<string, Block> _blocks = new Dictionary<string, Block>();
    static List<string> _blockKeys = new List<string>();    

    public static void AddBlock(Block block)
    {
        if( _blocks.ContainsKey(block.Id) ) { return; }

        _blocks.Add(block.Id, block);
        _blockKeys = _blocks.Keys.ToList();
    }

    public static Block GetBlock(string id)
    {
        if (!_blocks.ContainsKey(id)) { throw new KeyNotFoundException($"Block {id} not found in registry!"); }

        return _blocks[id];
    }
    public static Block GetBlock(int index)
    {
        var key = _blockKeys[index];
        var block = _blocks[key];

        if(block == null)
            throw new KeyNotFoundException($"Block at index {index} not found in registry!");

        return block;
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
