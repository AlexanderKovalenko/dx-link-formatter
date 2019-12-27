using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DXLinkFormatter {
    public partial class Form1 : Form {
        private static readonly Regex UrlRegex = new Regex(@"^(http|https|ms-help)\://[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,3})?(\:\d{1,5})?([\\/]\S*)?$", RegexOptions.IgnoreCase);
        private static string LinkFormat = "[{1}]({0})";
        private string lastText;

        private static string settingsPath = Application.StartupPath + "\\Settings.xml";

        List<LinkProcessingInfo> list = new List<LinkProcessingInfo>();

        public Form1() {
            InitializeComponent();

            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            formatLinksToolStripMenuItem.Checked = checkBox1.Checked;

            notifyIcon1.ShowBalloonTip(500);

            if (File.Exists(settingsPath)) {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(List<LinkProcessingInfo>));

                using (Stream fStream = new FileStream(settingsPath, FileMode.Open, FileAccess.Read)) {
                    list = (List<LinkProcessingInfo>)xmlFormat.Deserialize(fStream);
                }
            }

            dataGridView1.DataSource = new BindingList<LinkProcessingInfo>(list);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                if (Visible)
                    Hide();
                else {
                    Show();
                    Activate();
                    WindowState = FormWindowState.Normal;
                };
            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void formatLinksToolStripMenuItem_Click(object sender, EventArgs e) {
            formatLinksToolStripMenuItem.Checked = !formatLinksToolStripMenuItem.Checked;
            checkBox1.Checked = formatLinksToolStripMenuItem.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            formatLinksToolStripMenuItem.Checked = checkBox1.Checked;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Application.Exit();
        }

        private void clipboardMonitor1_ClipboardChanged(object sender, ClipboardChangedEventArgs e) {
            string clipboardText = e.DataObject.GetData(typeof(string)) as string;
            string title = string.Empty;

            if (!checkBox1.Checked)
                return;

            if (lastText == clipboardText)
                return;

            lastText = clipboardText;

            if (!string.IsNullOrEmpty(clipboardText) && UrlRegex.IsMatch(clipboardText))
                title = CalculateLinkTitle(new Uri(clipboardText));

            if (!string.IsNullOrEmpty(title)) {
                string currentFormattedLink = string.Format(LinkFormat, clipboardText, title.Trim());

                Clipboard.SetDataObject(currentFormattedLink, true, 10, 100);
            }
        }

        private string CalculateLinkTitle(Uri uri) {
            var result = "<EmptyTitle>";
            var host = uri.Host;

            var rule = list.Find(lpi => lpi.Host.Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase));

            if (rule != null) {
                if (!string.IsNullOrEmpty(rule.StaticTitle))
                    result = rule.StaticTitle;
                else if (rule.UseFragment) {
                    if (!string.IsNullOrEmpty(uri.Fragment)) {
                        result = uri.Fragment;
                    }
                    else {
                        result = uri.Segments[uri.Segments.Length - rule.SegmentIndex - 1];
                    }
                }
                else if (!rule.ParseTitleAttribute) {
                    result = uri.Segments[uri.Segments.Length - rule.SegmentIndex];
                }
            }

            if (result == "<EmptyTitle>") {
                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(uri);
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 30000;
                httpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                httpWebRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                try {
                    using (WebResponse response = httpWebRequest.GetResponse()) {
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {

                            result = GetTitleFromHtml(streamReader.ReadToEnd());

                            streamReader.Close();
                        }
                    }
                }
                catch (Exception ex) {
                    UIHelper.ShowInfo(ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(rule.RemoveChars)) {
                foreach (var chr in rule.RemoveChars) {
                    result = result.Replace(chr, ' ');
                }     
            }

            if (!string.IsNullOrEmpty(rule.ReplaceChars)) {
                for (int i = 0; i < rule.ReplaceChars.Length; i += 2) {
                    result = result.Replace(rule.ReplaceChars[i], rule.ReplaceChars[i + 1]);
                }
            }

            if (!string.IsNullOrEmpty(rule.TrimChars)) {
                result = result.Trim(rule.TrimChars.ToCharArray());
            }

            return result;
        }

        private string GetTitleFromHtml(string html) {
            string title = string.Empty;
            Match titleMatch = Regex.Match(html, @"<title>\s*(.+?)\s*</title>", RegexOptions.IgnoreCase);

            if (titleMatch.Success)
                title = titleMatch.Groups[1].Value;

            return title;
        }

        private void button1_Click(object sender, EventArgs e) {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(List<LinkProcessingInfo>));

            if (!File.Exists(settingsPath))
                File.Create(settingsPath).Close();

            using (Stream fStream = new FileStream(settingsPath, FileMode.Truncate, FileAccess.Write)) {
                xmlFormat.Serialize(fStream, list);
            }
        }
    }
}