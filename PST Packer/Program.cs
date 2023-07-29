using System;
using System.Diagnostics;
using System.Linq;
using PST_Packer;

class Program
{
    // bits to pack default is last 3
    // automated!
    static int BitsToPack = 3;
    // remember to add arrays in good spot to All[]
    static int PieceTypesCount = 6;
    // to get negative values make negative offset as packer doesnt check sign
    static int offset = -5;
    // if you need to multiply your values by static factor after adding offset use this
    static int multiplier = 1;
    // if you want different datatype make sure to update other functions
    static List<ulong> Packed = new();


    // Not important
    static Stopwatch stopWatch = new Stopwatch();
    static int calls = 0;

    static int[][][] All = new int[2][][] { new int[6][] { PST.PawnEarly, PST.KnightEarly, PST.BishopEarly, PST.RookEarly, PST.QueenEarly, PST.KingEarly }, new int[6][] { PST.PawnLate, PST.KnightLate, PST.BishopLate, PST.RookLate, PST.QueenLate, PST.KingLate } };

    static int Main()
    {
        var result = Test().ToString();
        string path = Path.Combine(Directory.GetCurrentDirectory(), "PST.cs");
        using StreamReader reader = new(path);
        string txt = reader.ReadToEnd();
        if (result != true.ToString())
        {
            Console.WriteLine($"Error:\n{result}");
            return 1;
        }
        Console.WriteLine("Everything went good\nTest was passed\n");
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
        Early = Early << 1 | GetBit(Packed[index], SquareIndex);
        Late = Late << 1 | GetBit(Packed[index + 1], SquareIndex);
    }}
    {returnString};
}}
static int GetBit(ulong number, int index) => (int)((number >> index) & 1);
";
        Console.WriteLine(output);
        Console.WriteLine("\nCopy this into your code and you should be good to go!");
        Console.WriteLine($"It contains {Packed.Count} values this results in {TokenCounter.CountTokens(output).totalCount} instead of approx {TokenCounter.CountTokens(txt).totalCount}\nYour limit is {BitsToPack}bits per value\nKeep in mind you can always add/subtract offset and multiply value returned by GetValue!");
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
                        current = current << 1;
                        current += GetBit(All[time][pieceType][squareIndex], bit);
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
            Early = Early << 1 | GetBit(Packed[index], SquareIndex);
            Late = Late << 1 | GetBit(Packed[index + 1], SquareIndex);
        }
        return (Early, Late);
    }

    static int GetBit(ulong number, int index) => (int)((number >> index) & 1);



    static ulong GetBit(int number, int index)
    {
        int mask = 1 << index;
        return (number & mask) != 0 ? 1UL : 0UL;
    }

    static int BitsToInt(string bits)
    {
        return Convert.ToInt32(bits, 2);
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
                        All[time][pieceType][squareIndex] = rng.Next(7);
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
                (int, int) correct = (All[0][pieceType][squareIndex], All[1][pieceType][squareIndex]);
                

                if (correct != returned)
                {
                    return new Failed(pieceType, squareIndex, returned, correct);
                }
            }
        }
        return true;
    }

    public struct Failed
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