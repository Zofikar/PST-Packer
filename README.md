# Simple program to pack PST tables into array of ulong
## Created to help to pack data for [Chess Challenge](https://github.com/SebLague/Chess-Challenge)

### How to use?
To use adjust multipier, offset, bits to pack in Program.cs and PST values in PST.cs

### It doesn't support negative values, to achieve them use offset and scale all values to be greater or equal to 0 !
For sake of simplicity just set your lowest value as offset and add to all values in PST.


### What about tokens usage?
It uses about 100 tokens depending on your offset and multipier + however many ulongs(or different data type if you change it) is needed to pack your data with your settings.
For example I use 2 stages * 6 piece types and I get 5 bits of it with offset -16 and multiplier of 5 my result is 161 tokens.
It will display exact amount of tokens as it uses [token counter from chess challange](https://github.com/SebLague/Chess-Challenge/tree/main/Chess-Challenge/src/Framework/Application/Helpers/Token%20Counter)

### How does it work?
It takes last bits of uints and adds them one by one into ulong untill its full then it adds ulong to list creates new ulong and repeats untill all values are inside ulong.
In Main() you can see call to Test() with default value of false which means that your values will be packed then unpacked and compared to raw data to ensure that everything is correct, to unpack it uses same function as it gives you.
