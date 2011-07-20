
/*
 * Reuters XML Search
 * 
 * Term.cs
 *
 * Encapsulation for a dictionar term along with its posting list.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;

namespace VectorModelIRS
{
    [Serializable]
    public class Term
    {
        public string Token { get; private set; }

        public List<int> PostingList { get; private set; }        

        public int Frequency { get; private set; }
        
        private List<int> _termDocumentFrequency;

        public Term(string token, int docId)
        {
            this.Token = token;
            this.PostingList = new List<int>();
            _termDocumentFrequency = new List<int>();
            this.PostingList.Add(docId);
            _termDocumentFrequency.Add(1);
            ++Frequency;
        }

        public void AddDocumentToPostingList(int docId)
        {
            this.PostingList.Add(docId);
            _termDocumentFrequency.Add(1);
            ++Frequency;
        }

        public int GetTermFrequencyInDocument(int docId)
        {
            try
            {
                var index = PostingList.BinarySearch(docId);
                return _termDocumentFrequency[index];
            }
            catch
            {
                return 0;
            }
        }

        public void IncrementTermFrequencyInDocument(int docId)
        {
            ++_termDocumentFrequency[PostingList.BinarySearch(docId)];
        }
    }
}
