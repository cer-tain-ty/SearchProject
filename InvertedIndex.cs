/*
 * Reuters XML Search
 * 
 * InvertedIndex.cs
 *
 * Encapsulation for an inverted index.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;
using VectorModelIRS;
namespace VectorModelIRS_2008
{
    [Serializable]
    public class InvertedIndex
    {
        public Dictionary<string, Term> Index { get; set; }
        public double[] MaxTermFrequencyInDocument { get; set; }
        public double[] DocumentLength { get; set; }
        public List<Document> Documents { get; set; }
    }

    [Serializable]
    public class Document
    {
        public int DocId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Date { get; set; }
    }
}
