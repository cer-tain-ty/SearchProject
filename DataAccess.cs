/*
 * Reuters XML Search
 * 
 * DataAccess.cs
 *
 * Provides access to database objects
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 * Made a change to the master branch directly.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using VectorModelIRS_2008;

namespace VectorModelIRS
{
    public static class DataAccess
    {
        static MetaDataDataContext _context = new MetaDataDataContext();

        public static List<Document> documents = new List<Document>();

        public static readonly bool UseDatabase = bool.Parse(ConfigurationManager.AppSettings["UseDatabase"]);

        public static void SaveToDatabase(IEnumerable<ReutersMetaDataDao> dbList)
        {
            if (UseDatabase)
            {
                _context.ReutersMetaDataDaos.InsertAllOnSubmit(dbList);
                _context.SubmitChanges();
            }
        }

        public static List<int> GetDocumentsBetweenDate(DateTime startDate, DateTime endDate)
        {
            if (UseDatabase)
                return _context.ReutersMetaDataDaos.Where(c => c.Date >= startDate.Date && c.Date <= endDate.Date).Select(c => c.DocId).OrderBy(c => c).ToList();
            else
            {
                return new List<int>();
            }
        }

        public static List<SearchResult> GetSearchResults(List<int> docIds, List<Term> terms)
        {
            if (UseDatabase)
            {
                var results = from c in _context.ReutersMetaDataDaos
                              where docIds.Contains(c.DocId)
                              select new SearchResult()
                              {
                                  Date = (DateTime)c.Date,
                                  DocumentId = c.DocId,
                                  Title = c.Title,
                                  Body = c.Body,
                                  Score = SearchLogic.CalculateScore(c.DocId, terms)
                              };

                return results.ToList();
            }
            else
            {
                List<SearchResult> results = new List<SearchResult>();
                foreach (var docId in docIds)
                {
                    var document = documents[docId - 1];
                    results.Add(new SearchResult()
                    {
                        DocumentId = document.DocId,
                        Date = document.Date,
                        Body = document.Body,
                        Title = document.Title,
                        Score = SearchLogic.CalculateScore(docId, terms)
                    });
                }

                return results;
            }
        }

        public static List<SearchResult> GetSearchResults(List<SearchResult> searchResults)
        {
            if (UseDatabase)
            {
                List<int> docIds = new List<int>();
                searchResults = searchResults.OrderBy(s => s.DocumentId).ToList();
                searchResults.ForEach(s => docIds.Add(s.DocumentId));
                var results = (from c in _context.ReutersMetaDataDaos
                               where docIds.Contains(c.DocId)
                               orderby c.DocId
                               select new
                               {
                                   Date = (DateTime)c.Date,
                                   DocumentId = c.DocId,
                                   Title = c.Title,
                                   Body = c.Body,
                               }).ToList();

                for (int i = 0; i < searchResults.Count; i++)
                {
                    searchResults[i].Date = results[i].Date;
                    searchResults[i].Title = results[i].Title;
                    searchResults[i].Body = results[i].Body;
                }
                return searchResults.OrderByDescending(s => s.Score).ToList();
            }
            else
            {
                foreach (var searchResult in searchResults)
                {
                    var document = documents[searchResult.DocumentId - 1];
                    searchResult.Date = document.Date;
                    searchResult.Body = document.Body;
                    searchResult.Title = document.Title;
                }

                return searchResults.OrderByDescending(s => s.Score).ToList();
            }
        }

        public static void DeleteAll()
        {
            if (UseDatabase)
            {
                _context.ReutersMetaDataDaos.DeleteAllOnSubmit(_context.ReutersMetaDataDaos.Select(s => s));
                _context.SubmitChanges();
            }
        }

        public static ReutersMetaDataDao GetDocument(int docId)
        {
            if (UseDatabase)
                return _context.ReutersMetaDataDaos.Single(d => d.DocId == docId);
            else
            {
                Document doc = documents[docId - 1];
                return new ReutersMetaDataDao()
                {
                    DocId = doc.DocId,
                    Title = doc.Title,
                    Date = doc.Date,
                    Body = doc.Body
                };
            }
        }
    }
}
