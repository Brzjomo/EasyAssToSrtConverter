using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EasyAssToSrtConverter
{
    public partial class Form1 : Form
    {
        private List<string> assFiles = [];
        private string[] supportedFormat = { "ass" };

        public Form1()
        {
            InitializeComponent();
        }

        private void LLB_AssFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (LLB_AssFolder.Text == string.Empty || LLB_AssFolder.Text == "未选择")
            {
                return;
            }
            else
            {
                if (Directory.Exists(LLB_AssFolder.Text))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer", LLB_AssFolder.Text);
                    }
                    catch
                    {
                        MessageBox.Show("路径错误！请确认路径是否存在！", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BTN_SelectAssFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LLB_AssFolder.Text = dialog.SelectedPath;
            }
        }

        private void GetAssFiles()
        {
            assFiles.Clear();

            if (Directory.Exists(LLB_AssFolder.Text))
            {
                string[] files = Directory.GetFiles(LLB_AssFolder.Text, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    if (FormatSupported(file))
                    {
                        assFiles.Add(file);
                    }
                }
            }
        }

        private bool FormatSupported(string file)
        {
            foreach (var item in supportedFormat)
            {
                if (file.EndsWith("." + item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void BTN_Run_Click(object sender, EventArgs e)
        {
            GetAssFiles();

            foreach (var ass in assFiles)
            {
                var name = Path.GetFileNameWithoutExtension(ass);
                var path = Path.GetDirectoryName(ass);

                path = path + "\\Output";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var srt = path + "\\" + name + ".srt";

                ConvertAssToSrt(ass, srt);
            }

            MessageBox.Show("转换完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ConvertAssToSrt(string assFilePath, string srtFilePath)
        {
            var lines = File.ReadAllLines(assFilePath);
            var srtLines = new List<string>();
            int index = 1;

            foreach (var line in lines)
            {
                if (line.StartsWith("Dialogue:"))
                {
                    var parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 9)
                    {
                        var startTime = ConvertAssTimeToSrtTime(parts[1]);
                        var endTime = ConvertAssTimeToSrtTime(parts[2]);
                        var text = string.Join(",", parts, 8, parts.Length - 8).Replace(@"\N", "\n").Trim();

                        text = Regex.Replace(text, @"\{[^}]*\}", string.Empty);

                        if (text.Contains("---"))
                        {
                            continue;
                        }

                        srtLines.Add(index.ToString());
                        srtLines.Add($"{startTime} --> {endTime}");
                        srtLines.Add(text);
                        srtLines.Add("");
                        index++;
                    }
                }
            }

            File.WriteAllLines(srtFilePath, srtLines);
            Debug.WriteLine("转换完成，输出文件：" + srtFilePath);
        }

        private string ConvertAssTimeToSrtTime(string assTime)
        {
            var match = Regex.Match(assTime, @"(\d+):(\d+):(\d+)\.(\d+)");
            if (match.Success)
            {
                return $"{match.Groups[1].Value}:{match.Groups[2].Value}:{match.Groups[3].Value}.{match.Groups[4].Value}";
            }
            return "00:00:00.000";
        }
    }
}
