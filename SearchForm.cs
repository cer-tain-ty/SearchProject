/*
 * Reuters XML Search
 * 
 * SearchForm.cs
 *
 * GUI serach screen.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using VectorModelIRS_2008;

namespace VectorModelIRS
{
    public partial class SearchForm : Form
    {
        public SearchForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.dataGridView1.AutoGenerateColumns = false;
            if (!DataAccess.UseDatabase)
            {
                dateTimePicker1.Enabled = dateTimePicker2.Enabled = checkBox1.Enabled = false;
            }
        }
        

        delegate void IndexerDelegate();
        private PorterStemmer _stemmer = new PorterStemmer();
        string[] _terms;
        string searchTerm;
        bool phraseSearch;
        Dictionary<string, Term> _invertedIndexDictionary = new Dictionary<string, Term>();
        double[] maxTermFrequencyInDocument;
        double[] documentLength;        

        private void button1_Click(object sender, EventArgs e)
        {
            string invertedFile = ConfigurationManager.AppSettings["InvertedFile"];
            string documentsFolder = textBox1.Text.Trim();
            IndexBuilder indexer = new IndexBuilder(documentsFolder, invertedFile, SetLabelText);                      
            IndexerDelegate index = new IndexerDelegate(indexer.GenerateIndex);          
            index.BeginInvoke(delegate(IAsyncResult ar)
                            {                                
                                _invertedIndexDictionary = indexer._invertedIndexDictionary;
                                maxTermFrequencyInDocument = indexer.maxTermFrequencyInDocument;
                                documentLength = indexer.documentLength;
                                if (!DataAccess.UseDatabase)
                                {
                                    DataAccess.documents = indexer.documents;
                                }
                                SetLabelText("", 0);
                            }, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_invertedIndexDictionary == null || _invertedIndexDictionary.Count == 0)
            {
                MessageBox.Show("Load the inverted index first before trying to search.");
                return;
            }

            label3.Text = "";
            dataGridView1.DataSource = null;
            searchTerm = textBox2.Text.Trim();
            phraseSearch = false;
            bool searchByDateAlso = checkBox1.Checked;

            if (string.IsNullOrEmpty(searchTerm))
            {

                label3.Text = "Please enter a search term.";
                return;
            }

            searchTerm = searchTerm.ToLower();

            //Full Text Search
            if (searchTerm.StartsWith("\"") && searchTerm.EndsWith("\""))
            {
                phraseSearch = true;
                searchTerm = searchTerm.Replace("\"", "");
            }           
            _terms = searchTerm.Split(IndexBuilder.SeparatorCharacters);
            List<Term> termSpecificResult = new List<Term>();
            foreach (var term in _terms)
            {
                var currenTterm = TextSanitizer.Sanitize(term);
                if (!string.IsNullOrEmpty(currenTterm) && !StopWords.IsStopWord(currenTterm))
                {
                    currenTterm = _stemmer.stemTerm(currenTterm);
                    if (_invertedIndexDictionary.ContainsKey(currenTterm))
                    {
                        var ter = _invertedIndexDictionary[currenTterm];                       
                        termSpecificResult.Add(ter);
                    }                   
                }                
            }

            if (termSpecificResult.Count > 0)
            {
                List<int> docListForDate = null;
                if (searchByDateAlso)
                {
                    docListForDate = DataAccess.GetDocumentsBetweenDate(dateTimePicker1.Value, dateTimePicker2.Value);
                    if (docListForDate == null || docListForDate.Count == 0)
                    {
                        label3.Text = "No documents where found between the given dates.";
                        return;
                    }                    
                }

                
                var sortedTermSpecificResult = termSpecificResult.OrderBy(t => t.PostingList.Count).ToList();
                List<int> docIds = sortedTermSpecificResult[0].PostingList;

                for (int i = 1; i < sortedTermSpecificResult.Count; i++)
                {
                    List<int> nextDocIds = sortedTermSpecificResult[i].PostingList;                    
                    docIds = SearchLogic.Intersect(docIds, nextDocIds);
                }

                if (docListForDate != null && docListForDate.Count > 0)
                {
                    docIds = SearchLogic.Intersect(docIds, docListForDate);
                }

                if (docIds.Count > 0)
                {
                    List<SearchResult> searchResults = DataAccess.GetSearchResults(docIds, sortedTermSpecificResult);
                    if (phraseSearch)
                    {                        
                        searchResults = searchResults.Where(sr => sr.Body.ToLower().Contains(searchTerm) || sr.Title.ToLower().Contains(searchTerm)).ToList();
                    }
                    
                    dataGridView1.DataSource = searchResults.OrderByDescending(s => s.Score).ToList();
                    string format = "The given search query matched {0} documents. Details shown in the table below.";
                    label3.Text = string.Format(format, searchResults.Count);
                }
                else
                {
                    label3.Text = "The given search query did not match any documents.";
                }
            }
            else
            {
                label3.Text = "The given search query did not match any documents.";
            }
        }

        private void SetLabelText(string text, int value)
        {            
            StatusLabel.Text = text;           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string invertedFile = ConfigurationManager.AppSettings["InvertedFile"];
            var index = BinarySerializer.DeSerializeObject<InvertedIndex>(invertedFile);
            _invertedIndexDictionary = index.Index;
            maxTermFrequencyInDocument = index.MaxTermFrequencyInDocument;
            documentLength = index.DocumentLength;
            if(!DataAccess.UseDatabase)
                DataAccess.documents = index.Documents;
            Cursor.Current = Cursors.Default;            
            MessageBox.Show("Index loaded sucessfully.");
        }
      

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            
            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            if (e.ColumnIndex == 0)
            {
                DataGridViewCheckBoxCell cell = row.Cells["Relevant"] as DataGridViewCheckBoxCell;
                if (cell.Value == cell.TrueValue)
                    cell.Value = cell.FalseValue;
                else
                    cell.Value = cell.TrueValue;

                return;
            }                        
            else
            {               
                int docId = Convert.ToInt32(row.Cells["DocumentId"].Value);
                Detail detailForm = new Detail(docId, _terms, searchTerm, phraseSearch);
                detailForm.Show();

                //foreach (DataGridViewRow row in dataGridView1.Rows)
                //{
                //    //Get the appropriate cell using index, name or whatever and cast to DataGridViewCheckBoxCell
                //    DataGridViewCheckBoxCell cell = row.Cells[colCheck] as DataGridViewCheckBoxCell;

                //    //Compare to the true value because Value isn't boolean
                //    if (cell.Value == cell.TrueValue)
                //    {

                //    }
                //    //The value is true
                //}


            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_invertedIndexDictionary == null || _invertedIndexDictionary.Count == 0)
            {
                MessageBox.Show("Load the inverted index first before trying to search.");
                return;
            }

            label3.Text = "";
            dataGridView1.DataSource = null;
            searchTerm = textBox2.Text.Trim();
            phraseSearch = false;
            bool searchByDateAlso = checkBox1.Checked;
            if (string.IsNullOrEmpty(searchTerm))
            {
                label3.Text = "Please enter a search term.";
                return;
            }

            searchTerm = searchTerm.ToLower();
            //Full Text Search
            if (searchTerm.StartsWith("\"") && searchTerm.EndsWith("\""))
            {
                phraseSearch = true;
                searchTerm = searchTerm.Replace("\"", "");
            }

            _terms = searchTerm.Split(IndexBuilder.SeparatorCharacters);
            List<Term> termSpecificResult = new List<Term>();
            foreach (var term in _terms)
            {
                var currenTterm = TextSanitizer.Sanitize(term);
                if (!string.IsNullOrEmpty(currenTterm) && !StopWords.IsStopWord(currenTterm))
                {
                    currenTterm = _stemmer.stemTerm(currenTterm);
                    if (_invertedIndexDictionary.ContainsKey(currenTterm))
                    {
                        var ter = _invertedIndexDictionary[currenTterm];
                        termSpecificResult.Add(ter);
                    }
                }
            }
            
            // Got list of terms, start calculation
            if (termSpecificResult.Count > 0)
            {
                List<int> docListForDate = null;
                if (searchByDateAlso)
                {
                    docListForDate = DataAccess.GetDocumentsBetweenDate(dateTimePicker1.Value, dateTimePicker2.Value);
                    if (docListForDate == null || docListForDate.Count == 0)
                    {
                        label3.Text = "No documents where found between the given dates.";
                        return;
                    }
                }

                List<SearchResult> searchResults = DataAccess.GetSearchResults( SearchLogic.CosineScore(termSpecificResult, 
                                                                                                200, 
                                                                                                maxTermFrequencyInDocument, 
                                                                                                documentLength, 
                                                                                                docListForDate));
                if (searchResults == null || searchResults.Count == 0)
                {
                    label3.Text = "The given search query did not match any documents.";
                    return;
                }

                if (phraseSearch)
                {
                    searchResults = searchResults.Where(sr => sr.Body.ToLower().Contains(searchTerm) || sr.Title.ToLower().Contains(searchTerm)).ToList();
                }
                
                dataGridView1.DataSource = searchResults;

                string format = "The given search query matched {0} documents. Details shown below.";

                if (searchResults.Count == 200)
                {
                    format = "Returning the top {0} documents. Details shown below.";
                }

                label3.Text = string.Format(format, searchResults.Count);                
            }
            else
            {
                label3.Text = "The given search query did not match any documents.";
            }
        }
    }
}
