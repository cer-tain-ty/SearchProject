
/*
 * Reuters XML Search
 * 
 * SearchResult.cs
 *
 * Encapsulation of a Search Result
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */


using System;

namespace VectorModelIRS
{
    public class SearchResult
    {
        public int DocumentId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public double Score { get; set; }
    }
}
