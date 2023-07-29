using System;
using System.Diagnostics;
using System.Linq;
using PST_Packer;

class Program
{
    // bits to pack default is last 5
    static int BitsToPack = 5;
    // remember to add arrays in good spot to All[]
    static int PieceTypesCount = 6;
    // to get negative values make negative offset as packer doesnt check sign
    // its default for 5 bits packed this lets us achieve result in range -16 to 15
    static int offset = -16;
    // if you need to multiply your values by static factor after adding offset use this
    // most likely your square values will be 0, 10, 20 ,25, 50, maybe some like -40 but hey you can achieve it by static multipier to save space in memory which will allow you to use less tokens
    static int multiplier = 5;
    // if you want different datatype make sure to update other functions
    static List<ulong> Packed = new();


    // Not important
    static Stopwatch stopWatch = new Stopwatch();
    static int calls = 0;

    static uint[][][] All = new uint[2][][] { new uint[6][] { PST.PawnEarly, PST.KnightEarly, PST.BishopEarly, PST.RookEarly, PST.QueenEarly, PST.KingEarly }, new uint[6][] { PST.PawnLate, PST.KnightLate, PST.BishopLate, PST.RookLate, PST.QueenLate, PST.KingLate } };

    static int Main()
    {
        var result = Test().ToString();
        if (result != true.ToString())
        {
            Console.WriteLine($"Error:\n{result}");
            return 1;
        }
        Console.WriteLine("Everything went good");
        Console.WriteLine("Test was passed");
        string path = Path.Combine(Directory.GetCurrentDirectory(), "PST.cs");
        using StreamReader reader = new(path);
        string txt = reader.ReadToEnd();
        string offsetString = (offset != 0 ? (offset > 0 ? $" + {offset}" : $" - {-offset}") : "");
        string returnString = (multiplier != 0 && multiplier != 1 ? $"return ( (Early{offsetString})*{multiplier}, (Late{offsetString})*{multiplier} )" : $"return ( Early{offsetString}, Late{offsetString} )");
        string output = $@"
static ulong[] Packed = {{
        {string.Join(",\n\t", Packed)}
}};
static (int, int) GetValue(int pieceType, int SquareIndex)
{{
    int Early = 0, Late = 0;
    for (int bitIndex = 0; bitIndex < {BitsToPack}; bitIndex++)
    {{
        var index = bitIndex * 2 + pieceType * ({BitsToPack} * 2);
        Early = Early << 1 | (int)((Packed[index] >> SquareIndex) & 1);
        Late = Late << 1 | (int)((Packed[index+1] >> SquareIndex) & 1);
    }}
    {returnString};
}}
";
        Console.WriteLine(output);
        Console.WriteLine();
        Console.WriteLine("Copy this into your code and you should be good to go!");
        Console.WriteLine($"It contains {Packed.Count} values this results in {TokenCounter.CountTokens(output).totalCount} instead of approx {TokenCounter.CountTokens(txt).totalCount}");
        Console.WriteLine($"Your limit is {BitsToPack}bits per value!");
        Console.WriteLine($"Average time to unpack single value for you is {new decimal(stopWatch.ElapsedTicks)/ calls / Stopwatch.Frequency * 1000}ms");
        Console.WriteLine($"Time to unpack all values for you is {new decimal(stopWatch.ElapsedTicks) / Stopwatch.Frequency * 1000}ms");
        return 0;
    }

    static void Pack()
    {
        Packed.Clear();
        for (int pieceType = 0; pieceType < PieceTypesCount; pieceType++)
        {
            for (int bit = BitsToPack - 1; bit >= 0; bit--)
            {
                for (int time = 0; time < 2; time++)
                {
                    ulong current = 0;
                    for (int squareIndex = 63; squareIndex >= 0; squareIndex--)
                    {
                        current = current << 1 | GetBit(All[time][pieceType][squareIndex], bit);
                    }
                    Packed.Add(current);
                }
            }
        }
    }

    static (int, int) GetValueWrapper(int pieceType, int SquareIndex)
    {
        calls++;
        stopWatch.Start();
        var result = GetValue(pieceType, SquareIndex);
        stopWatch.Stop();
        return result;
    }

    static (int, int) GetValue(int pieceType, int SquareIndex)
    {
        int Early = 0, Late = 0;
        for (int bitIndex = 0; bitIndex < BitsToPack; bitIndex++)
        {
            var index = bitIndex * 2 + pieceType * (BitsToPack * 2);
            Early = Early << 1 | (int)((Packed[index] >> SquareIndex) & 1);
            Late = Late << 1 | (int)((Packed[index+1] >> SquareIndex) & 1);
        }
        return (Early, Late);
    }


    static ulong GetBit(uint number, int index)
    {
        int mask = 1 << index;
        return (number & mask) != 0 ? 1UL : 0UL;
    }

    private static dynamic Test(bool makeRandom = false)
    {
        if (makeRandom)
        {
            Random rng = new Random();
            for (int time = 0; time < 2; time++)
            {
                for (int pieceType = 0; pieceType < PieceTypesCount; pieceType++)
                {
                    for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                    {
                        All[time][pieceType][squareIndex] = (uint)rng.Next(7);
                    }
                }
            }
        }
        Pack();
        for (int pieceType = 0; pieceType < PieceTypesCount; pieceType++)
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                (int, int) returned = GetValueWrapper(pieceType, squareIndex);
                (int, int) correct = ((int)All[0][pieceType][squareIndex], (int)All[1][pieceType][squareIndex]);
                

                if (correct != returned)
                {
                    return new Failed(pieceType, squareIndex, returned, correct);
                }
            }
        }
        return true;
    }

    public readonly struct Failed
    {
        public Failed(int pieceType, int squareIndex, (int, int) returnedValue, (int, int) correctValue)
        {
            this.pieceType = pieceType;
            this.squareIndex = squareIndex;
            this.returnedValue = returnedValue;
            this.correctValue = correctValue;
        }
        public readonly int pieceType;
        public readonly int squareIndex;
        public readonly (int, int) returnedValue;
        public readonly (int, int) correctValue;

        public override readonly string ToString() => $"Piece Type: {pieceType}\nSquare Index: {squareIndex}\nReturned Value: {returnedValue}\nCorrect Value: {correctValue}";
    }
}