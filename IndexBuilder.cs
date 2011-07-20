/*
 * Reuters XML Search
 * 
 * IndexBuilder.cs
 *
 * Builds the inverted index.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using VectorModelIRS_2008;
using System.Configuration;

namespace VectorModelIRS
{
    public delegate void SetText(string text, int value);
    
    public class IndexBuilder
    {
        const string FileNameFormat = "reut2-0{0}.xml";
        readonly List<int> _range = Enumerable.Range(0, 21).ToList();
        public static readonly char[] SeparatorCharacters = { ' ', '/', ',', '.' };
        PorterStemmer _stemmer = new PorterStemmer();       
        public Dictionary<string, Term> _invertedIndexDictionary = new Dictionary<string, Term>();
        string _sourceFolder;
        string _invertedFilePath;
        Label _statusLabel;
        SetText SetLabelText;
        List<ReutersMetaDataDao> dbList = new List<ReutersMetaDataDao>();
        public List<Document> documents = new List<Document>();
        public double[] maxTermFrequencyInDocument = new double[21000];
        public double[] documentLength = new double[21000];
        private bool _indexTitle = bool.Parse(ConfigurationManager.AppSettings["IndexTitle"]);

        public IndexBuilder(string sourceFolder, string invertedFile, SetText setLabelText)
        {
            _sourceFolder = sourceFolder;
            _invertedFilePath = invertedFile;
            SetLabelText = setLabelText;                 
        }

        
        public void GenerateIndex()
        {
            DateTime startIndexTime = DateTime.Now;
            _range.ForEach(i => ProcessFile(i));
            DateTime endIndexTime = DateTime.Now;
            TimeSpan indexSpan = endIndexTime.Subtract(startIndexTime);

            DateTime startSerializationTime = DateTime.Now;

            InvertedIndex index = new InvertedIndex();
            index.Index = _invertedIndexDictionary;
            index.MaxTermFrequencyInDocument = maxTermFrequencyInDocument;
            index.DocumentLength = documentLength;
            if (!DataAccess.UseDatabase)
            {
                index.Documents = documents;
            }
            long objectSize = BinarySerializer.SerializeObject<InvertedIndex>(_invertedFilePath, index);
            DateTime endSerializationTime = DateTime.Now;
            TimeSpan serializationSpan = endSerializationTime.Subtract(startSerializationTime);

            MessageBox.Show(string.Format("Index generated successfully.\nIndexing Time: {0} sec.\nSerialization Time: {1} sec.\nSize of Index: {2} bytes.", indexSpan.Seconds, serializationSpan.Seconds, objectSize));

            SaveToDatabase();

        }


        private void SaveToDatabase()
        {
            if (DataAccess.UseDatabase)
            {
                try
                {
                    DataAccess.SaveToDatabase(dbList);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        private void ProcessFile(int i)
        {
            string index = (i <= 9) ? string.Concat("0", i) : i.ToString();
            string fileName = string.Format(FileNameFormat, index);

            SetLabelText.BeginInvoke("Processing " + fileName,i + 1, null, null);

            string fullPath = Path.Combine(_sourceFolder, fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);

            var lst = doc.SelectNodes("/LEWIS/REUTERS");

            foreach (XmlNode node in lst)
            {

                var titleNode = node.SelectSingleNode("TEXT/TITLE");
                Dictionary<string, int> documentDictionary = new Dictionary<string, int>();
                var docId = int.Parse(node.Attributes["NEWID"].InnerText.Trim());
                string title = string.Empty;

                if (titleNode != null)
                {
                    title = titleNode.InnerText.Trim();
                    if(_indexTitle)
                        ProcessText(title, docId, documentDictionary);
                }

                var textNode = node.SelectSingleNode("TEXT/BODY");
                string body = string.Empty;
                if (textNode != null)
                {
                    body = textNode.InnerText.Trim();
                    ProcessText(body, docId, documentDictionary);
                }

                var dateNode = node.SelectSingleNode("DATE");
                string date = string.Empty;

                if (dateNode != null)
                {
                    date = dateNode.InnerText.Trim();
                }

                if (DataAccess.UseDatabase)
                {
                    dbList.Add(new ReutersMetaDataDao()
                    {
                        DocId = docId,
                        Title = title,
                        Body = body,
                        Date = DateTime.Parse(date.Split(' ')[0])
                    });
                }
                else
                {
                    documents.Add(new Document()
                    {
                        DocId = docId,
                        Title = title,
                        Body = body,
                        Date = DateTime.Parse(date.Split(' ')[0])
                    });
                }

                if (documentDictionary.Count != 0)
                {
                    maxTermFrequencyInDocument[docId - 1] = documentDictionary.Max(t => t.Value);
                    documentLength[docId - 1] = documentDictionary.Sum(t => t.Value);
                }

               // documentDetailsList.Add(new SearchResult() { Date = date, DocumentId = docId, Title = title, Body = body });

            }
            
        }

        private void ProcessText(string text, int docId, Dictionary<string, int> documentDictionary)
        {
            text = text.Replace("\n", " ");
            string[] words = text.Split(SeparatorCharacters);
            foreach (var word in words)
            {
                var lowerWord = word.Trim().ToLower();

                var cleanedText = TextSanitizer.Sanitize(lowerWord);

                if (!string.IsNullOrEmpty(cleanedText) && !StopWords.IsStopWord(cleanedText))
                {
                    var stemmedWord = _stemmer.stemTerm(cleanedText);

                    if (!_invertedIndexDictionary.ContainsKey(stemmedWord))
                    {
                        _invertedIndexDictionary.Add(stemmedWord, new Term(stemmedWord, docId));
                        documentDictionary.Add(stemmedWord, 1);
                    }
                    else
                    {
                        var term = _invertedIndexDictionary[stemmedWord];
                        if (!documentDictionary.ContainsKey(stemmedWord))
                        {
                            documentDictionary.Add(stemmedWord, 1);
                            term.AddDocumentToPostingList(docId);
                        }
                        else
                        {
                            ++documentDictionary[stemmedWord];
                            term.IncrementTermFrequencyInDocument(docId);
                        }
                    }
                }

            }
        }
    }
}
