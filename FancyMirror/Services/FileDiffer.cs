using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiffMatchPatch;
using FancyMirror.DataModels;

namespace FancyMirror.Services;

public class FileDiffer
{
    #region Private Fields

    /// <summary>
    /// collection of diff results
    /// </summary>
    private readonly ObservableCollection<DiffedFileContent> _diffedFiles;
    
    #endregion
    
    

    #region Constructor
    
    /// <summary>
    /// creates a file differ service.
    /// </summary>
    public FileDiffer()
    {
        this._diffedFiles = new ObservableCollection<DiffedFileContent>();
    }
    
    #endregion

    
    
    #region Public Methods

    /// <summary>
    /// gets a list of diffed content.
    /// </summary>
    /// <returns>list of diffed content</returns>
    public ObservableCollection<DiffedFileContent> GetDiffedFiles() => this._diffedFiles;

    /// <summary>
    /// generates diffs between files with the same name in both directories.
    /// </summary>
    /// <param name="uri1">first directory</param>
    /// <param name="uri2">second directory</param>
    public void DiffCommonFilesFromDirs(Uri uri1, Uri uri2)
    {
        string dir1LocalPath = uri1.LocalPath; // convert the URI paths to local paths
        string dir2LocalPath = uri2.LocalPath;

        if (!Directory.Exists(dir1LocalPath) || !Directory.Exists(dir2LocalPath))
            return;
        
        // get the common names of the `.md` files
        var filesInDir1 = Directory.EnumerateFiles(dir1LocalPath, "*.md")
            .Select(Path.GetFileName)
            .ToList();
        var filesInDir2 = Directory.EnumerateFiles(dir2LocalPath, "*.md")
            .Select(Path.GetFileName)
            .ToList();
        var commonFiles = filesInDir1.Intersect(filesInDir2).ToList();

        foreach (var fileName in commonFiles)
        {
            if (fileName == null)
                continue;
            
            string file1Path = Path.Combine(dir1LocalPath, fileName);
            string file2Path = Path.Combine(dir2LocalPath, fileName);

            if (File.Exists(file1Path) && File.Exists(file2Path))
            {
                string contentFile1 = File.ReadAllText(file1Path);
                string contentFile2 = File.ReadAllText(file2Path);

                var diffs = DiffWordMode(contentFile1, contentFile2);
                var taggedContent = Diffs2TaggedString(diffs);
                
                _diffedFiles.Add(new DiffedFileContent(fileName, taggedContent));
            }
        }
    }

    #endregion

    

    #region Private Methods
    
    /// <summary>
    /// generates word-level diffs. 
    /// </summary>
    /// <param name="text1">original text</param>
    /// <param name="text2">new text, to be diffed with the original one</param>
    /// <returns>list of diffs</returns>
    private List<Diff> DiffWordMode(string text1, string text2)
    {
        var dmp = new diff_match_patch();

        (string charsEncoded1, string charsEncoded2, List<string> wordsList) = DiffWords2Chars(text1, text2);
        
        List<Diff> diffs = dmp.diff_main(charsEncoded1, charsEncoded2, false);
        dmp.diff_cleanupSemantic(diffs);

        DiffChars2Words(diffs, wordsList);
        
        return diffs;
    }

    
    /// <summary>
    /// convert a sequence of words to a sequence of chars.
    /// </summary>
    /// <param name="text1">original text</param>
    /// <param name="text2">new text, to be diffed with the original</param>
    /// <returns>a tuple containing the encoded versions of the 2 input strings and a list of words</returns>
    private Tuple<string, string, List<string>> DiffWords2Chars(string text1, string text2)
    {
        var wordsList = new List<string>();
        var wordsHash = new Dictionary<string, int>();
        
        // convert words to chars
        string chars1 = WordsToCharsMunge(text1, wordsList, wordsHash);
        string chars2 = WordsToCharsMunge(text2, wordsList, wordsHash);
        
        return Tuple.Create(chars1, chars2, wordsList);
    }
    
    
    /// <summary>
    /// map words-to-characters and convert words to characters
    /// </summary>
    /// <param name="text">the text which needs to have the words mapped to chars</param>
    /// <param name="wordsList">list to which the words will be added</param>
    /// <param name="wordsHash">dict for storing hashes used to map words to chars</param>
    /// <returns>string with words encoded as chars</returns>
    private string WordsToCharsMunge(string text, List<string> wordsList, Dictionary<string, int> wordsHash)
    {
        var chars = new StringBuilder();
        var wordRegex = new Regex(@"\b\S+\b|\s+|[^\w\s]+");

        foreach (Match match in wordRegex.Matches(text))
        {
            string word = match.Value;
                
            if (wordsHash.ContainsKey(word))
            {
                chars.Append((char)wordsHash[word]);
            }
            else
            {
                wordsList.Add(word);
                wordsHash[word] = wordsList.Count - 1;
                chars.Append((char)(wordsList.Count - 1));
            }
        }
        return chars.ToString();
    }

    
    /// <summary>
    /// convert char encoded diffs to word diffs.
    /// </summary>
    /// <param name="diffs">char encoded diffs list</param>
    /// <param name="wordsList">list of encoded words</param>
    private void DiffChars2Words(List<Diff> diffs, List<string> wordsList)
    {
        foreach (var diff in diffs)
        {
            var text = new StringBuilder();
            
            foreach (char c in diff.text) // convert each character back to the original word
            {
                int wordIndex = c;
                if (wordIndex >= 0 && wordIndex < wordsList.Count)
                {
                    text.Append(wordsList[wordIndex]);
                }
            }
            
            diff.text = text.ToString();
        }
    }

    
    /// <summary>
    /// convert diff list to tagged string.
    /// </summary>
    /// <param name="diffs">list of diffs</param>
    /// <returns>a string of concatenated diffs, with tags `old` and `new` for removed and added content</returns>
    private string Diffs2TaggedString(List<Diff> diffs)
    {
        List<string> processed = new List<string>();

        foreach (var diff in diffs)
        {
            switch (diff.operation)
            {
                case Operation.INSERT: // new, inserted text
                    processed.Add($"<new>{diff.text}</new>");
                    break;
                case Operation.DELETE: // old, deleted text
                    processed.Add($"<old>{diff.text}</old>");
                    break;
                case Operation.EQUAL: // common text
                    processed.Add(diff.text);
                    break;
            }
        }

        return string.Join(string.Empty, processed); // join the list
    }
    
    #endregion

}