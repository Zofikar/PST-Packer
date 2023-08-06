using PST_Packer;
using MoreLinq;

class Program
{ 
    static int[] pesto = PST.mg_pawn_table.Concat(PST.eg_pawn_table).Concat(PST.mg_knight_table).Concat(PST.eg_knight_table).Concat(PST.mg_bishop_table).Concat(PST.eg_bishop_table).Concat(PST.mg_rook_table).Concat(PST.eg_rook_table)
        .Concat(PST.mg_queen_table).Concat(PST.eg_queen_table).Concat(PST.mg_king_table).Concat(PST.eg_king_table).ToArray();
    static int Main()
    {
        decimal[] packedPesto = PackPesto(pesto);
        int[] unpackedPesto = Unpack(packedPesto);
        pesto[128] = -167;
        pesto[149] = 129;
        for(int i = 0; i < unpackedPesto.Length; i++)
        {
            if (unpackedPesto[i] != pesto[i]) Console.WriteLine(string.Format("{0} != {1}",unpackedPesto[i], pesto[i]));
        }
        Console.WriteLine(string.Join("m,\n", packedPesto)+'m');
        return 0;
    }

    static decimal[] PackPesto(int[] intArray)
    {
        intArray[128] = 0;
        intArray[149] = 0;
        return intArray.Select((x, i) => (byte)Convert.ToSByte(x - (i < 128 ? 64 : 0))).Batch(4).Select( x => BitConverter.ToInt32(x.ToArray())).Batch(3).Select(x =>
        {
            var t = x.ToList();
            t.Add(0xF0000);
            return new decimal(t.ToArray());
        }).ToArray();
    }



    static int[] Unpack(decimal[] pestoPacked)
    {
        int[] unpackedPesto = pestoPacked.SelectMany(x => decimal.GetBits(x).Take(3)).SelectMany(BitConverter.GetBytes).Select((x, i) => (sbyte)x + (i < 128 ? 64 : 0)).ToArray();
        unpackedPesto[128] = -167;
        unpackedPesto[149] = 129;
        return unpackedPesto;
    }
}