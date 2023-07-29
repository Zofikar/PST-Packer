using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using PST_Packer;

class Program
{
    //bits to pack default is last 3
    static int BitsToPack = 4;
    // remember to add arrays in good spot to All[]
    static int PieceTypesCount = 6;
    static List<ulong> Packed = new();
    //BigInteger val = (BigInteger)320265757102059730318470218759311257840;
    static Stopwatch stopWatch = new Stopwatch();
    static int calls = 0; 

    static int[][][] All = new int[2][][] { new int[6][] { PST.PawnEarly, PST.KnightEarly, PST.BishopEarly, PST.RookEarly, PST.QueenEarly, PST.KingEarly }, new int[][] { PST.PawnLate, PST.KnightLate, PST.BishopLate, PST.RookLate, PST.QueenLate, PST.KingLate } };

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
        string output = $@"
static int BitsToPack = {BitsToPack};
static int PieceTypesCount = {PieceTypesCount};
static ulong[] Packed = {{
    {string.Join(",\n\t", Packed)}
}};
static (int, int) GetValue(int pieceType, int SquareIndex)
{{
    int Early = 0;
    int Late = 0;
    ulong bit;
    for (int bitIndex = 0; bitIndex < BitsToPack; bitIndex++)
    {{
        var index = bitIndex*2+pieceType*(BitsToPack*2);
        bit = Packed[index];
        Early <<= 1;
        Early += GetBit(bit, SquareIndex);
        bit = Packed[index+1];
        Late <<= 1;
        Late += GetBit(bit, SquareIndex);
    }}
    return (Early, Late);
}}
static int GetBit(ulong number, int index)
{{
    ulong mask = 1UL << index;
    return (number & mask) != 0 ? 1 : 0;
}}
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

    static (int, int) GetValue(int pieceType, int SquareIndex)
    {
        calls++;
        stopWatch.Start();
        int Early = 0;
        int Late = 0;
        ulong bit;
        for (int bitIndex = 0; bitIndex < BitsToPack; bitIndex++)
        {
            var index = bitIndex*2+pieceType*(BitsToPack*2);
            bit = Packed[index];
            Early <<= 1;
            Early += GetBit(bit, SquareIndex);
            bit = Packed[index+1];
            Late <<= 1;
            Late += GetBit(bit, SquareIndex);
        }
        stopWatch.Stop();
        return (Early, Late);
    }

    static int GetBit(ulong number, int index)
    {
        ulong mask = 1UL << index;
        return (number & mask) != 0 ? 1 : 0;
    }

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
                (int, int) returned = GetValue(pieceType, squareIndex);
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