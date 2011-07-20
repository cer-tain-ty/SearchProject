
/*
 * Reuters XML Search
 * 
 * StopWords.cs
 *
 * Provides details about StopWords
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;

namespace VectorModelIRS
{
    public static class StopWords
    {
        private static Dictionary<string, string> _stopWordDictionary = new Dictionary<string, string>();
        static StopWords()
        {
            string[] stopWord = {"a", "an", "and", "are", "as", "at", "be", "by", "for", "from", 
                                 "has", "he", "in", "is", "it", "its", "of", "on", "that", "the", 
                                 "to", "was", "were", "will", "with" };

            Array.ForEach(stopWord, s => _stopWordDictionary.Add(s, s));
        }


        public static bool IsStopWord(string word)
        {
            return _stopWordDictionary.ContainsKey(word);
        }
    }
}