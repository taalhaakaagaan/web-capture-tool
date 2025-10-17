using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;

namespace CSharpApp
{
public class MainForm : Form
{
    private TextBox urlTextBox = null!;
    private Button scrapeButton = null!;
    private RichTextBox resultTextBox = null!;
    private PictureBox screenshotBox = null!;
    private SplitContainer splitContainer = null!;        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Web Content Scraper";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(640, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Create URL panel
            var urlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var urlLabel = new Label
            {
                Text = "URL:",
                AutoSize = true,
                Location = new Point(10, 20),
            };

            urlTextBox = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(60, 17),
                Size = new Size(urlPanel.Width - 170, 23),
                Font = new Font("Segoe UI", 9F),
                Text = "https://example.com"
            };

            scrapeButton = new Button
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(urlPanel.Width - 100, 16),
                Size = new Size(90, 25),
                Text = "Scrape",
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            scrapeButton.Click += ScrapeButton_Click;

            urlPanel.Controls.AddRange(new Control[] { urlLabel, urlTextBox, scrapeButton });

            // Create split container for content and screenshot
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = this.Height / 2,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            // Content panel (top)
            var resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            resultTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = true
            };

            resultPanel.Controls.Add(resultTextBox);
            splitContainer.Panel1.Controls.Add(resultPanel);

            // Screenshot panel (bottom)
            var screenshotPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            var screenshotLabel = new Label
            {
                Text = "Screenshot Preview (Right-click to save)",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Padding = new Padding(0, 0, 0, 5)
            };

            screenshotBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            screenshotPanel.Controls.AddRange(new Control[] { screenshotLabel, screenshotBox });
            splitContainer.Panel2.Controls.Add(screenshotPanel);

            // Status strip
            var statusStrip = new StatusStrip
            {
                Name = "statusStrip",
                BackColor = Color.FromArgb(240, 240, 240)
            };
            var statusLabel = new ToolStripStatusLabel("Ready");
            statusStrip.Items.Add(statusLabel);

            // Add all controls to form
            this.Controls.AddRange(new Control[] { urlPanel, splitContainer, statusStrip });
        }

        private async void ScrapeButton_Click(object? sender, EventArgs e)
        {
            string url = urlTextBox.Text;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter a valid URL", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
                urlTextBox.Text = url;
            }

            scrapeButton.Enabled = false;
            var statusLabel = (this.Controls.Find("statusStrip", true)[0] as StatusStrip)?.Items[0] as ToolStripStatusLabel;
            if (statusLabel != null) statusLabel.Text = "Scraping...";
            resultTextBox.Text = "Please wait, fetching content...";
            Cursor = Cursors.WaitCursor;

            try
            {
                // Use built-in WebBrowser control (IE engine) to do a headful load and capture
                var captureResult = await CaptureUrlHeadful(url, 1280, 960);

                if (!captureResult.Success)
                {
                    resultTextBox.Text = $"Error occurred while scraping:\n{captureResult.ErrorMessage}";
                    if (statusLabel != null) statusLabel.Text = "Error occurred";
                }
                else
                {
                    resultTextBox.Text = FormatHtml(captureResult.CleanedContent ?? "");

                    if (!string.IsNullOrEmpty(captureResult.ScreenshotFileName))
                    {
                        string screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
                        string screenshotPath = Path.Combine(screenshotsDir, captureResult.ScreenshotFileName);
                        if (File.Exists(screenshotPath))
                        {
                            using (var stream = new MemoryStream(File.ReadAllBytes(screenshotPath)))
                            {
                                screenshotBox.Image?.Dispose();
                                screenshotBox.Image = Image.FromStream(stream);
                            }
                        }
                    }

                    if (statusLabel != null) statusLabel.Text = "Scraping completed successfully";
                }
            }
            catch (Exception ex)
            {
                resultTextBox.Text = $"Application error:\n{ex.Message}";
                if (statusLabel != null) statusLabel.Text = "Application error occurred";
            }
            finally
            {
                scrapeButton.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // Helper: result container
        public class CaptureResult
        {
            public bool Success { get; set; }
            public string? CleanedContent { get; set; }
            public string? ScreenshotFileName { get; set; }
            public string? ErrorMessage { get; set; }
        }

        // Ensure IE11 emulation for the process
        private void EnsureBrowserEmulation()
        {
            try
            {
                var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(exeName);
                        if (val == null || (int)val != 11001)
                        {
                            key.SetValue(exeName, 11001, RegistryValueKind.DWord); // IE11 mode
                        }
                    }
                }
            }
            catch { /* ignore */ }
        }

    // Capture a URL using a hidden WebBrowser control (runs on UI thread)
    public Task<CaptureResult> CaptureUrlHeadful(string url, int width, int height)
        {
            var tcs = new TaskCompletionSource<CaptureResult>();

            try
            {
                EnsureBrowserEmulation();

                var web = new WebBrowser
                {
                    ScriptErrorsSuppressed = true,
                    ScrollBarsEnabled = false,
                    Width = width,
                    Height = height
                };

                // place off-screen but still in controls so DrawToBitmap works
                web.Location = new Point(-width * 2, -height * 2);
                this.Controls.Add(web);

                WebBrowserDocumentCompletedEventHandler handler = null!;
                handler = async (s, e) =>
                {
                    // Ignore sub-frame document completed events
                    try
                    {
                        if (e.Url != web.Url)
                            return;

                        // Wait for meaningful content (ready state + body/title heuristics)
                        bool gotContent = await WaitForMeaningfulContent(web, timeoutMs: 60000, pollMs: 500);

                        using (var bmp = new Bitmap(web.Width, web.Height))
                        {
                            web.DrawToBitmap(bmp, new Rectangle(0, 0, web.Width, web.Height));

                            // prepare screenshots folder
                            string screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
                            Directory.CreateDirectory(screenshotsDir);
                            string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
                            string fullPath = Path.Combine(screenshotsDir, filename);
                            bmp.Save(fullPath, ImageFormat.Png);

                            // Prefer the document body text for cleaned content; fallback to DocumentText
                            var cleaned = (web.Document?.Body?.InnerText ?? web.DocumentText ?? string.Empty);

                            // Don't dispose yet - just remove the handler
                            web.DocumentCompleted -= handler;

                            if (gotContent)
                                tcs.SetResult(new CaptureResult { Success = true, CleanedContent = FormatHtml(web.DocumentText ?? cleaned), ScreenshotFileName = filename });
                            else
                                tcs.SetResult(new CaptureResult { Success = true, CleanedContent = FormatHtml(web.DocumentText ?? cleaned), ScreenshotFileName = filename, ErrorMessage = "Captured but content may be incomplete (timeout)" });
                            
                            // Cleanup will happen when form closes
                        }
                    }
                    catch (Exception ex)
                    {
                        try { web.DocumentCompleted -= handler; } catch { }
                        tcs.SetResult(new CaptureResult { Success = false, ErrorMessage = ex.Message });
                    }
                };

                web.DocumentCompleted += handler;

                // Navigate and allow the handler's async wait logic to decide when to snapshot
                web.Navigate(url);
            }
            catch (Exception ex)
            {
                tcs.SetResult(new CaptureResult { Success = false, ErrorMessage = ex.Message });
            }

            return tcs.Task;
        }

        // Poll the WebBrowser for meaningful content: readyState complete plus heuristics on title/body content.
        private async Task<bool> WaitForMeaningfulContent(WebBrowser web, int timeoutMs = 60000, int pollMs = 500)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Retry navigation attempts if we detect repeated small placeholder content
            int attempts = 0;
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    // Check ready state
                    if (web.ReadyState != WebBrowserReadyState.Complete)
                    {
                        await Task.Delay(pollMs);
                        continue;
                    }

                    string title = web.Document?.Title ?? string.Empty;
                    string body = web.Document?.Body?.InnerText ?? string.Empty;
                    string html = web.DocumentText ?? string.Empty;

                    // Heuristics: non-trivial title or body length indicates page has loaded meaningful content
                    bool hasTitle = title.Trim().Length > 5;
                    bool hasBody = body.Trim().Length > 250; // allow some JS-heavy pages more time

                    // Detect common loading placeholders and consider them not meaningful
                    string low = body.ToLowerInvariant();
                    bool looksLikeLoading = low.Contains("loading") || low.Contains("please wait") || low.Contains("spinner") || low.Contains("authorizing") || low.Contains("verifying") || low.Contains("checking");

                    if ((hasTitle || hasBody) && !looksLikeLoading)
                        return true;

                    // If body is tiny and attempts are low, give it more time; on repeated tiny bodies try a reload
                    if (body.Trim().Length < 50 && attempts < 2)
                    {
                        attempts++;
                        try { web.Refresh(); } catch { }
                    }
                }
                catch { /* ignore transient DOM inspection errors */ }

                await Task.Delay(pollMs);
            }

            return false; // timed out
        }

        private string FormatHtml(string html)
        {
            // Basic HTML formatting
            int indent = 0;
            bool inTag = false;
            var formatted = new System.Text.StringBuilder();

            foreach (char c in html)
            {
                switch (c)
                {
                    case '<':
                        inTag = true;
                        if (formatted.Length > 0 && formatted[formatted.Length - 1] != '\n')
                            formatted.AppendLine();
                        formatted.Append(new string(' ', indent * 2)).Append(c);
                        if (formatted.Length >= 2 && formatted[formatted.Length - 2] == '<' && formatted[formatted.Length - 1] == '/')
                            indent--;
                        break;
                    case '>':
                        inTag = false;
                        formatted.Append(c);
                        if (formatted.Length >= 2 && formatted[formatted.Length - 2] != '/' && formatted[formatted.Length - 1] == '>')
                            indent++;
                        formatted.AppendLine();
                        break;
                    default:
                        if (!inTag && c == ' ' && formatted.Length > 0 && formatted[formatted.Length - 1] == ' ')
                            continue;
                        formatted.Append(c);
                        break;
                }
            }

            return formatted.ToString();
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var args = Environment.GetCommandLineArgs();

            // CLI mode: capture a URL and print JSON
            if (args.Length >= 3 && (args[1] == "--capture-url" || args[1] == "-c"))
            {
                string url = args[2];
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Run a hidden MainForm which will perform capture on Load and then exit
                var form = new MainForm();
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-20000, -20000);

                form.Shown += async (s, e) =>
                {
                    // perform capture and print JSON to stdout
                    WebBrowser[] browsers = Array.Empty<WebBrowser>();
                    try
                    {
                        var result = await form.CaptureUrlHeadful(url, 1280, 960);
                        var output = new System.Collections.Generic.Dictionary<string, object>();
                        output["success"] = result.Success;
                        if (!string.IsNullOrEmpty(result.ScreenshotFileName)) output["screenshot"] = result.ScreenshotFileName;
                        if (!string.IsNullOrEmpty(result.CleanedContent)) output["data"] = result.CleanedContent;
                        if (!string.IsNullOrEmpty(result.ErrorMessage)) output["error"] = result.ErrorMessage;

                        string json = System.Text.Json.JsonSerializer.Serialize(output);

                        // Find all WebBrowser controls for proper cleanup
                        browsers = form.Controls.OfType<WebBrowser>().ToArray();

                        // Write JSON to file first (more reliable than stdout in GUI process)
                        try
                        {
                            string screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
                            Directory.CreateDirectory(screenshotsDir);
                            string baseName = (result.ScreenshotFileName ?? "capture").Replace(".png", "");
                            string jsonFile = Path.Combine(screenshotsDir, baseName + ".json");
                            File.WriteAllText(jsonFile, json);
                        }
                        catch { /* ignore file write errors */ }

                        // Try stdout after file write
                        try
                        {
                            Console.WriteLine(json);
                        }
                        catch { /* ignore console write errors for GUI-hosted processes */ }
                    }
                    finally
                    {
                        // In CLI mode, exit after capture
                        await Task.Delay(250);
                        form.BeginInvoke(new Action(() =>
                        {
                            // Clean up the capture browser but leave the form open
                            foreach (var browser in browsers)
                            {
                                try 
                                { 
                                    form.Controls.Remove(browser);
                                    browser.Dispose();
                                }
                                catch { /* ignore cleanup errors */ }
                            }
                            
                            // Only close and exit in CLI mode
                            if (args.Length >= 3 && (args[1] == "--capture-url" || args[1] == "-c"))
                            {
                                form.Close();
                                Application.ExitThread();
                            }
                        }));
                    }
                };

                Application.Run(form);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}