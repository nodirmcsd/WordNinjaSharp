Word Ninja C#
==========

C#.Net Library for splitting concatenated words, using NLP based on English Wikipedia unigram frequencies.

![wordninjasharp-250](https://user-images.githubusercontent.com/1277302/229291849-d5b1b9c3-ee41-4722-a5db-c00aaa51909e.jpg)

This project is repackaging the excellent work from here: http://stackoverflow.com/a/11642687/2449774

This project is C#.Net version of https://github.com/keredson/wordninja

Use this library for slicing damaged data, etc.


Usage
-----

```csharp
using WordNinjaSharp.App;

var result = WordNinja.Split("thequickbrownfoxjumpsover1978thelazydog");
Console.WriteLine(string.Join(" ", result));

//the quick brown fox jumps over 1978 the lazy dog

```

Performance
-----
The fastest then all other!!! 


```csharp

var sw = Stopwatch.StartNew();

var res = WordNinja.Split("denythyfatherandrefusethyname");

Console.WriteLine($"{sw.ElapsedMilliseconds} ms");

Console.WriteLine(string.Join(" ", res));

//182 ms
//deny thy father and refuse thy name

```        

Install
-----

```
dotnet add package WordNinjaSharp 
```

or 

```
Install-Package WordNinjaSharp
```

Custom dictionary 
-----
```csharp
var path = @"path/to/your/words/list/gzarchive/or/txtfile";
var res = WordNinja.Split("denythyfatherandrefusethyname", path);
```
