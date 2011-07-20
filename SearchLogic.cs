
/*
 * Reuters XML Search
 * 
 * SearchLogic.cs
 *
 * Contains details of the Search Implementation.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorModelIRS
{
    public class SearchLogic
    {
        const double NumberOfDocuments = 21000.0;
        const double a = 0.4;

        public static List<int> Intersect(List<int> list1, List<int> list2)
        {
            return list1.Intersect(list2).ToList();
        }

        public static List<int> Union(List<int> list1, List<int> list2)
        {
            return list1.Union(list2).ToList();
        }

        public static double InverseDocumentFrequency(Term t)
        {
            return Math.Log10(NumberOfDocuments / t.Frequency);
        }

        public static double CalculateScore(int docId, List<Term> terms)
        {
            double score = 0.0;

            foreach (Term term in terms)
            {
                int termFrequency = term.GetTermFrequencyInDocument(docId);//term.Documents.Find(doc => doc.DocId == docId).Frequency;
                double tfIdf = termFrequency * InverseDocumentFrequency(term);
                score += tfIdf;
            }

            return score;
        }

        public static double GetNormalizedTermFrequencyInDocument(Term term, int docId, double[] maxTermFrequencyInDocument)
        {

            try
            {
                int tf = term.GetTermFrequencyInDocument(docId);
                if (tf != 0)
                {
                    return (a + (1 - a) * (term.GetTermFrequencyInDocument(docId) / maxTermFrequencyInDocument[docId - 1]));
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static List<SearchResult> CosineScore(List<int> docIds, List<Term> terms, int k, double[] maxTermFrequencyInDocument, double[] documentLengths)
        {
            int n = docIds.Count;
            List<SearchResult> searchResults = new List<SearchResult>();
            Dictionary<int, SearchResult> docIdSearchResultsDict = new Dictionary<int, SearchResult>();
            docIds.ForEach(docId => docIdSearchResultsDict.Add(docId, new SearchResult(){DocumentId = docId}));
            foreach (Term term in terms)
            {
                var wtq = InverseDocumentFrequency(term);
                //foreach (int docId in term.PostingList)
                foreach (int docId in docIds)
                {
                    var searchResult = docIdSearchResultsDict[docId];
                    double wftd = GetNormalizedTermFrequencyInDocument(term, docId, maxTermFrequencyInDocument);
                    searchResult.Score += wtq * wftd;
                }
            }

            docIdSearchResultsDict.Values.ToList().ForEach(s => s.Score = s.Score / documentLengths[s.DocumentId - 1]);
            searchResults = docIdSearchResultsDict.Values.OrderByDescending(s => s.Score).Take(k).ToList();
                        
            return searchResults;
        }

        public static List<SearchResult> CosineScore(List<Term> terms, int k, double[] maxTermFrequencyInDocument, double[] documentLengths, List<int> docListForDate)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            Dictionary<int, SearchResult> docIdSearchResultsDict = new Dictionary<int, SearchResult>();
            
            foreach (Term term in terms)
            {
                var wtq = InverseDocumentFrequency(term);
                foreach (int docId in term.PostingList)
                {
                    if (!docIdSearchResultsDict.ContainsKey(docId))
                    {
                        docIdSearchResultsDict.Add(docId, new SearchResult() { DocumentId = docId });
                    }

                    var searchResult = docIdSearchResultsDict[docId];
                    double wftd = GetNormalizedTermFrequencyInDocument(term, docId, maxTermFrequencyInDocument);                    
                    searchResult.Score += wtq * wftd;
                }
            }

            docIdSearchResultsDict.Values.ToList().ForEach(s => s.Score = s.Score / documentLengths[s.DocumentId - 1]);

            if (docListForDate != null && docListForDate.Count > 0)
            {
                searchResults = docIdSearchResultsDict.Values.Join(docListForDate, d => d.DocumentId, d => d, (c, d) => c)
                                                             .OrderByDescending(s => s.Score)
                                                             .Take(k)
                                                             .ToList();
            }
            else
            {
                searchResults = docIdSearchResultsDict.Values.OrderByDescending(s => s.Score)
                                                             .Take(k)
                                                             .ToList();
            }

            return searchResults;
        }
    }
}
