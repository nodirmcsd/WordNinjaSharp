using System.IO.Compression;
using System.Reflection;

namespace WordNinjaSharp.App;

/// <summary>
///     WordNinja
/// </summary>
public class WordNinja
{
    private const int InputMaxLength = 2000;
    private const int WordsMaxCount = 600000;
    private static int _maxWord;
    private static Dictionary<string, double> _wordCost = new();
    private static WordNinja? _instance;

    /// <summary>
    ///     WordNinja
    /// </summary>
    /// <param name="path"></param>
    private WordNinja(string path)
    {
        LoadDictionary(path);
    }


    /// <summary>
    ///     Splits damaged words
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="dictionaryPath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string Split(string inputString, string? dictionaryPath = null)
    {
        dictionaryPath ??= GetDefaultDictionaryPath();
        if (string.IsNullOrEmpty(inputString))
            throw new ArgumentException($"{nameof(inputString)} is not specified");
        if (inputString.Length > InputMaxLength)
            throw new ArgumentException($"{nameof(inputString)} is too long. MaxLength = {InputMaxLength}");
        if (!File.Exists(dictionaryPath))
            throw new ArgumentException($"dictionary file is not found. {nameof(dictionaryPath)} = {dictionaryPath}");

        _instance ??= new WordNinja(dictionaryPath);
        return _instance.SplitToWords(CleanSourceString(inputString));
    }

    /// <summary>
    ///  Gets default dictionary path
    /// </summary>
    /// <returns></returns>
    private static string GetDefaultDictionaryPath()
    {
        var currentExecutePath = Assembly.GetExecutingAssembly().Location;
        var currentExecuteDirectory = Path.GetDirectoryName(currentExecutePath)!;
        return Path.Combine(currentExecuteDirectory, "wordninja.words.txt.gz");
    }

    #region Private Methods

    /// <summary>
    ///     Adds words to dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="line"></param>
    private static void AddWords(ref List<string> dictionary, string? line)
    {
        if (string.IsNullOrEmpty(line)) return;
        var w = Tokenize(line).ToList();
        if (w.Any()) dictionary.AddRange(w);
    }


    /// <summary>
    ///     Cleans source string 
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    private static string CleanSourceString(string src)
    {
        return string.Join("", src.Where(x => char.IsLetterOrDigit(x) || x == '\''));
    }

    /// <summary>
    ///     Finds the best match
    /// </summary>
    /// <param name="i"></param>
    /// <param name="cost"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public Tuple<double, int> BestMatch(int i, IReadOnlyList<double> cost, string s)
    {
        var candidates = Enumerable
            .Range(Math.Max(0, i - _maxWord), i)
            .Reverse()
            .Select((c, index) => new { Index = index, Cost = cost[c] });
        return candidates.Select(c =>
                Tuple.Create(c.Cost +
                             _wordCost.GetValueOrDefault(
                                 s.Substring(i - c.Index - 1, c.Index + 1).ToLowerInvariant(), double.PositiveInfinity),
                    c.Index + 1))
            .Min()!;
    }

    private string SplitToWords(string? inputString)
    {
        inputString = inputString?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(inputString))
            throw new ArgumentException($"{nameof(inputString)} is not specified");
        var cost = new double[inputString.Length + 1];
        cost[0] = 0;
        for (var i = 1; i <= inputString.Length; i++)
        {
            var bestMatch = BestMatch(i, cost, inputString);
            cost[i] = bestMatch.Item1;
        }

        var outList = new List<string>();
        var j = inputString.Length;
        while (j > 0)
        {
            var bestMatch = BestMatch(j, cost, inputString);
            var isNewToken = true;
            var str = inputString.Substring(j - bestMatch.Item2, bestMatch.Item2);
            if (outList.Any())
            {
                if (outList[^1] == "'s")
                {
                    outList[^1] = str + outList[^1];
                    isNewToken = false;
                }

                if (str == "'")
                {
                    outList[^1] = str + outList[^1];
                    isNewToken = false;
                }

                if (char.IsDigit(str[^1]) && char.IsDigit(outList[^1][^1]))
                {
                    outList[^1] = str[^1] + outList[^1];
                    isNewToken = false;
                }

                if (outList[^1].Length == 1 && outList[^1] == "s")
                {
                    outList[^1] = str + outList[^1];
                    isNewToken = false;
                }
            }

            if (isNewToken)
                outList.Add(inputString.Substring(j - bestMatch.Item2, bestMatch.Item2));
            j -= bestMatch.Item2;
        }

        outList.Reverse();
        return string.Join(" ", outList);
    }

    /// <summary>
    ///     Loads dictionary 
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    private static void LoadDictionary(string path)
    {
        if (!File.Exists(path))
            throw new ArgumentException($"dictionary file is not found. {nameof(path)} = {path}");
        var words = new List<string>();
        try
        {
            using var fileStream = new FileStream(path, FileMode.Open);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gzipStream);
            while (!streamReader.EndOfStream) AddWords(ref words, streamReader.ReadLine());
        }
        catch
        {
            words = new List<string>();
        }

        try
        {
            if (!words.Any())
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines) AddWords(ref words, line);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"can't load dictionary from specified path {path}. Error message={ex.Message}");
        }

        if (!words.Any())
            throw new Exception("no words loaded");
        if (words.Count > WordsMaxCount)
            throw new Exception($"the maximum number of words in dictionary = {WordsMaxCount}");
        _wordCost = words
            .Distinct()
            .Select((word, index) => new
            {
                Key = word,
                Value = Math.Log((index + 1) * Math.Log(words.Count))
            })
            .ToDictionary(x => x.Key, x => x.Value);
        _maxWord = words.Max(word => word.Length);
    }

    /// <summary>
    /// Gets tokens
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private static IEnumerable<string> Tokenize(string line)
    {
        return line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
    }

    #endregion
}