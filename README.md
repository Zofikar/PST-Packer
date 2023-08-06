# Simple program to pack PST tables into array of decimals
## Created to help to pack data for [Chess Challenge](https://github.com/SebLague/Chess-Challenge)
Edit values in PST.cs to match your needs then copy output and unpack function into your code please note now its set to match default values from [chessprogramming.org](https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function) so adjust if your values exceed sbyte range.
## Access data
To access data after unpacking just use (128 * ((int)p.PieceType - 1)) + square.index for mg values and for eg values add 64 to this