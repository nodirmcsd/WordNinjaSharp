# WordNinjaSharp

C#.Net Library for spliting concatenated words, using NLP based on English Wikipedia unigram frequencies.

![wordninjasharp-250](https://user-images.githubusercontent.com/1277302/229291849-d5b1b9c3-ee41-4722-a5db-c00aaa51909e.jpg)

This project is repackaging the excellent work from here: http://stackoverflow.com/a/11642687/2449774

This project is C#.Net version of https://github.com/keredson/wordninja

```
using WordNinjaSharp.App;
var result = WordNinja.Split("thequickbrownfoxjumpsover1978thelazydog");
Console.WriteLine(string.Join(" ", result));

//Will print "the quick brown fox jumps over 1978 the lazy dog"


```
