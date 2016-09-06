using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CharacterFinder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

        }

        private static bool IsVailed(string txt)
        {
            /*
             * Japan character systen unicode range
             * U + 4E00–U + 9FBF Kanji
             * U + 3040–U + 309F Hiragana
             * U + 30A0–U + 30FF Katakana
             * return text.Any(c => c >= 0x20000 && c <= 0xFA2D);
             * */

            return !txt.Any(c =>
                c >= 0x4E00 && c <= 0x9FBF ||
                c >= 0x3040 && c <= 0x309F ||
                c >= 0x30A0 && c <= 0x30FF
                );
        }

        private static IList<string> FindNonAlphaNumericLines(IList<string> lines)
        {
            var result = new List<string>();
            if (lines == null || !lines.Any())
            {
                return result;
            }

            var count = 0;
            foreach (var line in lines)
            {
                count++;
                if (IsVailed(line))
                {
                    continue;
                }

                result.Add($"{count}: {line}");
            }

            return result;
        }

        private static string RemoveComment(string input)
        {
            const string blockComments = @"/\*(.*?)\*/";
            const string lineComments = @"//(.*?)\r?\n";
            const string strings = @"""((\\[^\n]|[^""\n])*)""";
            const string verbatimStrings = @"@(""[^""]*"")+";

            var noComments = Regex.Replace(input,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                    {
                        // return me.Value.StartsWith("//") ? Environment.NewLine : "";
                        return string.Empty;
                    }

                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return noComments;
        }

        private static IList<string> ReadFile(string path)
        {
            var lines = new List<string>();
            var result = new List<string>();
            if (!File.Exists(path))
            {
                return lines;
            }

            var src = File.ReadAllText(path);
            src = RemoveComment(src);
            var srcAry = src.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            lines.AddRange(srcAry);

            result.AddRange(FindNonAlphaNumericLines(lines));
            return result;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            var path = textBoxDir.Text;

            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            richTextBoxLog.Clear();
            var reslut = new List<string>();
            foreach (var file in files)
            {
                var tmp = ReadFile(file);
                if (!tmp.Any())
                {
                    continue;
                }

                reslut.Add(file + Environment.NewLine);
                reslut.AddRange(tmp);
            }

            foreach (var txt in reslut)
            {
                richTextBoxLog.AppendText(txt + Environment.NewLine);
            }
        }

        private void buttonFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxDir.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
