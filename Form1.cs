using System.Diagnostics;
using System.Management; // Add at the top with other using directives
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DownloadYTChannel
{
    public partial class Form1 : Form
    {
        private Process ytDlpProcess;
        public string limit_rate = "--limit-rate 5M";
        private bool calledProgrammatically = false;

        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon("icon.ico");
            textBoxOutput.ReadOnly = true;
        }

        private List<Process> GetChildProcesses(int parentId)
        {
            List<Process> children = new List<Process>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    $"SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = {parentId}"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        int childId = Convert.ToInt32(mo["ProcessId"]);
                        try
                        {
                            Process child = Process.GetProcessById(childId);
                            children.Add(child);
                        }
                        catch
                        {
                            // process might have exited already
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendOutput("Error retrieving child processes: " + ex.Message);
            }
            return children;
        }

        private string cleanInput(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                return string.Empty;

            string input = channel.Trim();
            string channelName;

            // 1) Try to match “@username” in any URL or raw text
            var atMatch = Regex.Match(input, @"@(?<user>[^\/\?\&]+)");
            if (atMatch.Success)
            {
                channelName = atMatch.Groups["user"].Value;
            }
            else if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                // 2) No @ — take the FIRST segment of the path
                //    e.g. "/hanswelder/videos" -> ["hanswelder","videos"] -> "hanswelder"
                var trimmedPath = uri.AbsolutePath.Trim('/');            // "hanswelder/videos" or "hanswelder"
                var segments = trimmedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                channelName = segments.Length > 0
                                  ? segments[0]
                                  : string.Empty;
            }
            else
            {
                // 3) Not a URL at all — treat entire trimmed input as the username
                channelName = input;
            }

            return channelName;
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            string channelname = cleanInput(textBoxChannel.Text.Trim());

            if (checkBox1.Checked)
            {
                limit_rate = "";
            }

            if (string.IsNullOrWhiteSpace(channelname))
            {
                MessageBox.Show("Please enter a YouTube channel.");
                return;
            }

            buttonStart.Enabled = false;
            buttonClose.Enabled = false;
            textBoxOutput.Clear();

            await Task.Run(() => RunDownloadAndGenerate(channelname));

            buttonStart.Enabled = true;
            buttonClose.Enabled = true;
        }
        private void RunDownloadAndGenerate(string channel)
        {
            string ytDlpExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp_win7.exe");
            if (!File.Exists(ytDlpExe))
            {
                AppendOutput("yt-dlp_win7.exe not found in application directory.");
                return;
            }

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, channel);
            Directory.CreateDirectory(folder);

            string args = $"--write-description --write-info-json --write-thumbnail --embed-thumbnail --download-archive \"{folder}\\downloaded.txt\" --merge-output-format mkv --sleep-interval 1 --max-sleep-interval 2 --user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36\" -N 1 {limit_rate} -P \"{folder}\" https://www.youtube.com/@{channel}";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ytDlpExe,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.GetEncoding(850), // or Encoding.GetEncoding(1252)
                StandardErrorEncoding = Encoding.GetEncoding(850)
            };

            AppendOutput($"▶ Starting yt-dlp for channel @{channel}...\r\n");

            ytDlpProcess = new Process { StartInfo = psi };

            ytDlpProcess.OutputDataReceived += (s, e) => AppendOutput(e.Data);
            ytDlpProcess.ErrorDataReceived += (s, e) => AppendOutput(e.Data);

            ytDlpProcess.Start();
            ytDlpProcess.BeginOutputReadLine();
            ytDlpProcess.BeginErrorReadLine();

            // Wait briefly to allow child processes to spawn
            Thread.Sleep(1000);

            // List child processes
            var children = GetChildProcesses(ytDlpProcess.Id);
            if (children.Count > 0)
            {
                AppendOutput("Child processes of yt-dlp:");
                foreach (var child in children)
                {
                    AppendOutput($" - {child.ProcessName} (PID: {child.Id})");
                }
            }
            else
            {
                AppendOutput("No child processes detected (yet).");
            }

            ytDlpProcess.WaitForExit();

            if (ytDlpProcess.ExitCode == 0)
            {
                AppendOutput("Download complete. Generating HTML gallery...\r\n");

                try
                {
                    GenerateHtmlGallery(folder, channel);
                    AppendOutput("HTML gallery created.\r\n");
                }
                catch (Exception ex)
                {
                    AppendOutput("Error creating HTML: " + ex.Message);
                }
            }

            ytDlpProcess = null; // Clean up
        }


        private void GenerateHtmlGallery(string dir, string channel)
        {
            string htmlFile = Path.Combine(dir, "index.html");
            using (StreamWriter writer = new StreamWriter(htmlFile, false, Encoding.UTF8))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html><head><meta charset='UTF-8'>");
                writer.WriteLine($"<title>{channel} Youtube Video Gallery</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("table { width: 100%; border-collapse: collapse; }");
                writer.WriteLine("td { padding: 10px; border: 1px solid #ccc; vertical-align: top; width: 16.66%; }");
                writer.WriteLine("img { width: 220px; height: auto; display: block; }");
                writer.WriteLine(".title { font-weight: bold; }");
                writer.WriteLine(".description { font-size: 0.9em; color: #555; word-wrap: break-word; }");
                writer.WriteLine("a { color: blue; word-break: break-word; }");
                writer.WriteLine("</style></head><body>");
                writer.WriteLine($"<h1>{channel} Youtube Video Gallery</h1><table>");

                var entries = new SortedList<string, string>(Comparer<string>.Create((a, b) => b.CompareTo(a))); // newest first
                foreach (var jsonFile in Directory.GetFiles(dir, "*.info.json"))
                {
                    string date = GetUploadDate(jsonFile);
                    if (date != null)
                        entries[date + jsonFile] = jsonFile;
                }

                int rowIndex = 0;
                int entryIndex = 0;

                foreach (var json in entries.Values)
                {
                    //if (entryIndex == 1) { entryIndex++; continue; }

                    string baseName = Path.GetFileNameWithoutExtension(json).Replace(".info", "");
                    string videoFile = FindFile(dir, baseName, new[] { ".mkv", ".mp4", ".webm" });
                    string thumbFile = FindFile(dir, baseName, new[] { ".jpg", ".jpeg", ".png", ".webp" });

                    if (videoFile == null || thumbFile == null) { entryIndex++; continue; }

                    string title = ExtractJsonValue(json, "title") ?? baseName;
                    string description = ExtractJsonValue(json, "description") ?? "";

                    //if (description.Contains("Filmaufnahmen"))
                    //   MessageBox.Show(description);

                    //string cleanedDescription = Regex.Replace(description, @"[\r\n\u000D\u000A]+", " ");
                    //string shortDesc = WrapText(cleanedDescription.Replace("\r", "").Replace("\n", " ").Replace("\\n", " ").Replace("\\\"", "\""), 36, 50000);
                    //shortDesc = WebUtility.HtmlEncode(shortDesc);


                    string normalized = description.Replace("\\n", "\n");
                    string shortDesc = normalized.Replace("\\\"", "&quot;");

                    // Assuming shortDesc contains your input string
                    string pattern = @"(https?://[^\s""'<>]+)";
                    string replacement = "<a href=\"$1\" target=\"_blank\">$1</a>";

                    // Replace URLs in shortDesc with anchor tags
                    shortDesc = Regex.Replace(shortDesc, pattern, replacement);

                    /*
                    // Step 1: Normalize line endings (optional if all are \n)
                    string cleaned = Regex.Replace(htmlSafe, @"(\r\n|\r|\n)", "\n");

                    // Step 2: HTML-encode to prevent HTML injection
                    string encoded = WebUtility.HtmlEncode(cleaned);

                    // Step 3: Replace newlines with <br> for browser display
                    string shortDesc = encoded.Replace("\n", "<br>");
                    */

                    string videoUrl = videoFile.Replace("#", "%23");
                    string thumbUrl = thumbFile.Replace("#", "%23");

                    if (rowIndex % 4 == 0)
                        writer.WriteLine("<tr>");

                    writer.WriteLine("<td>");
                    writer.WriteLine($"<a href=\"{Path.GetFileName(videoUrl)}\">");
                    writer.WriteLine($"<img src=\"{Path.GetFileName(thumbUrl)}\" alt=\"{title}\"></a>");
                    writer.WriteLine($"<div class=\"title\">{title}</div>");
                    writer.WriteLine($"<div class=\"description\">{shortDesc}</div>");
                    writer.WriteLine("</td>");

                    rowIndex++;
                    if (rowIndex % 4 == 0)
                        writer.WriteLine("</tr>");

                    entryIndex++;
                }

                if (rowIndex % 4 != 0)
                    writer.WriteLine("</tr>");

                writer.WriteLine("</table></body></html>");
            }
        }

        private string GetUploadDate(string jsonFile)
        {
            string content = File.ReadAllText(jsonFile);
            int i = content.IndexOf("\"upload_date\"");
            if (i == -1) return null;
            int start = content.IndexOf('"', i + 14) + 1;
            int end = content.IndexOf('"', start);
            return content.Substring(start, end - start);
        }

        /*private string ExtractJsonValue(string jsonFile, string key)
        {
            string content = File.ReadAllText(jsonFile);
            int i = content.IndexOf($"\"{key}\"");
            if (i == -1) return null;
            int start = content.IndexOf('"', i + key.Length + 3) + 1;
            int end = content.IndexOf('"', start);
            return content.Substring(start, end - start);
        }*/

        private string ExtractJsonValue(string jsonFile, string key)
        {
            string content = File.ReadAllText(jsonFile);
            int i = content.IndexOf($"\"{key}\"");
            if (i == -1) return null;

            // Find the first quote after the colon
            int start = content.IndexOf('"', i + key.Length + 3) + 1;
            if (start == 0) return null; // No starting quote found

            int end = start;
            bool escape = false;

            while (end < content.Length)
            {
                if (escape)
                {
                    escape = false;
                }
                else if (content[end] == '\\')
                {
                    escape = true;
                }
                else if (content[end] == '"')
                {
                    break;
                }
                end++;
            }

            if (end >= content.Length) return null;

            return content.Substring(start, end - start);
        }


        private string WrapText(string text, int width, int maxLines)
        {
            var sb = new StringBuilder();
            int count = 0;
            foreach (var line in SplitEvery(text, width))
            {
                if (++count > maxLines) break;
                sb.Append(line + "<br>");
            }
            return sb.ToString();
        }

        private IEnumerable<string> SplitEvery(string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }

        private string FindFile(string dir, string baseName, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                var match = Directory.GetFiles(dir, baseName + "*" + ext);
                if (match.Length > 0)
                    return match[0];
            }
            return null;
        }

        private void AppendOutput(string text)
        {
            if (text == null) return;
            if (textBoxOutput.InvokeRequired)
            {
                textBoxOutput.Invoke((MethodInvoker)(() =>
                {
                    textBoxOutput.AppendText(text + "\r\n");
                }));
            }
            else
            {
                textBoxOutput.AppendText(text + "\r\n");
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private async void buttonGenerateOnly_Click(object sender, EventArgs e)
        {
            string channel = cleanInput(textBoxChannel.Text.Trim());

            if (string.IsNullOrWhiteSpace(channel))
            {
                MessageBox.Show("Please enter a YouTube channel.");
                return;
            }

            buttonStart.Enabled = false;
            buttonGenerateOnly.Enabled = false;
            buttonClose.Enabled = false;
            textBoxOutput.Clear();

            await Task.Run(() =>
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, channel);
                if (!Directory.Exists(folder))
                {
                    AppendOutput($"Folder \"{folder}\" not found. Run a download first or check the channel name.");
                    return;
                }

                try
                {
                    AppendOutput("Generating HTML gallery...");
                    GenerateHtmlGallery(folder, channel);
                    AppendOutput("HTML gallery created.");
                }
                catch (Exception ex)
                {
                    AppendOutput("Error creating HTML: " + ex.Message);
                }
            });

            buttonStart.Enabled = true;
            buttonGenerateOnly.Enabled = true;
            buttonClose.Enabled = true;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.AcceptButton = buttonStart;
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 20000;
            toolTip1.SetToolTip(this.buttonGenerateOnly, "Skip downloading. If you already downloaded the channel and the files are in the folder with the given channel-name, just generate the index.html!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ytDlpProcess != null && !ytDlpProcess.HasExited)
            {
                try
                {
                    //ytDlpProcess.Kill(); // Ends the process
                    var psi = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {ytDlpProcess.Id} /T /F",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(psi);

                    textBoxOutput.AppendText("DOWNLOAD VIDEOS CANCELLED.\r\n");
                }
                catch (Exception ex)
                {
                    textBoxOutput.AppendText("Failed to cancel download: " + ex.Message + "\r\n");
                }
            }
            else
            {
                if (!calledProgrammatically)
                    textBoxOutput.AppendText("No download in progress.\r\n");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/nicolaasjan/yt-dlp";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Required to open the URL in the default browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open URL: " + ex.Message);
            }
        }

        private void textBoxOutput_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Disable the entire form so nothing can be clicked
            button2.Text = "Wait...";
            this.Enabled = false;

            calledProgrammatically = true;

            button1_Click(this, EventArgs.Empty); // halt yt-dlp_win7

            string url = "https://github.com/nicolaasjan/yt-dlp/releases/latest/download/yt-dlp_win7.exe";
            string targetFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp_win7.exe");

            try
            {
                using HttpClient client = new HttpClient();

                // Download and overwrite existing file
                using HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // throws if not success

                using FileStream fs = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);

                MessageBox.Show("Download complete and file updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                button2.Text = "Update yt-dlp";
            }
        }
    }
}
