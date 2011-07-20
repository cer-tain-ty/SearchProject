/*
 * Reuters XML Search
 * 
 * Detail.cs
 *
 * GUI Detail screen.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VectorModelIRS;

namespace VectorModelIRS_2008
{
    public partial class Detail : Form
    {

        private int _docId;

        private string[] _terms;
        private string _phrase;
        private bool _phraseSearch;        
        
        public Detail(int docId, string[] terms, string phrase, bool phraseSearch)
        {
            _docId = docId;
            _terms = terms;
            _phrase = phrase;
            _phraseSearch = phraseSearch;
            InitializeComponent();
        }

        private void Detail_Load(object sender, EventArgs e)
        {
            var result = DataAccess.GetDocument(_docId);
            richTextBox1.Text = result.Title;
            richTextBox2.Text = result.Body;

            this.Text = "Detail for DocId : " + _docId.ToString();

            if (!_phraseSearch)
            {
                foreach (var term in _terms)
                {
                    if (!StopWords.IsStopWord(term))
                    {
                        MatchCollection titleMatches = Regex.Matches(richTextBox1.Text.ToLower(), term);

                        foreach (Match match in titleMatches)
                        {
                            HighLightText(richTextBox1, match);
                        }

                        MatchCollection bodyMatches = Regex.Matches(richTextBox2.Text.ToLower(), term);

                        foreach (Match match in bodyMatches)
                        {
                            HighLightText(richTextBox2, match);
                        }
                    }
                }
            }
            else
            {
                MatchCollection titleMatches = Regex.Matches(richTextBox1.Text.ToLower(), _phrase);

                foreach (Match match in titleMatches)
                {
                    HighLightText(richTextBox1, match);
                }

                MatchCollection bodyMatches = Regex.Matches(richTextBox2.Text.ToLower(), _phrase);

                foreach (Match match in bodyMatches)
                {
                    HighLightText(richTextBox2, match);
                }
            }
            
        }

        private void HighLightText(RichTextBox txtBox, Match match)
        {
            txtBox.Select(match.Index, match.Length);
            txtBox.SelectionBackColor = Color.Yellow;
            txtBox.SelectionLength = 0;
        }
    }
}
