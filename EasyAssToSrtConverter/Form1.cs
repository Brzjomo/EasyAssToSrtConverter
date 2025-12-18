using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyAssToSrtConverter
{
    public partial class Form1 : Form
    {
        // 使用 HashSet 避免重复，且查找更快
        private List<string> assFiles = new List<string>();
        private readonly string[] supportedFormat = { ".ass" }; // 统一带点

        public Form1()
        {
            InitializeComponent();
        }

        private void LLB_AssFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(LLB_AssFolder.Text) || LLB_AssFolder.Text == "未选择")
                return;

            if (Directory.Exists(LLB_AssFolder.Text))
            {
                try
                {
                    Process.Start("explorer.exe", LLB_AssFolder.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法打开路径: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BTN_SelectAssFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LLB_AssFolder.Text = dialog.SelectedPath;
                }
            }
        }

        // 修改为异步事件处理
        private async void BTN_Run_Click(object sender, EventArgs e)
        {
            string folderPath = LLB_AssFolder.Text;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("请先选择有效的目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BTN_Run.Enabled = false; // 禁用按钮防止重复点击
            BTN_Run.Text = "转换中...";

            try
            {
                GetAssFiles(folderPath);

                if (assFiles.Count == 0)
                {
                    MessageBox.Show("该目录下没有找到 .ass 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 在后台线程运行转换逻辑，避免界面卡死
                await Task.Run(() =>
                {
                    foreach (var ass in assFiles)
                    {
                        try
                        {
                            string folder = Path.GetDirectoryName(ass) ?? folderPath;
                            string name = Path.GetFileNameWithoutExtension(ass);

                            // 使用 Path.Combine 处理路径
                            string outputDir = Path.Combine(folder, "Output");
                            if (!Directory.CreateDirectory(outputDir).Exists)
                            {
                                Directory.CreateDirectory(outputDir);
                            }

                            string srtPath = Path.Combine(outputDir, name + ".srt");
                            ConvertAssToSrt(ass, srtPath);
                        }
                        catch (Exception ex)
                        {
                            // 实际项目中可以记录日志
                            Debug.WriteLine($"文件 {ass} 转换失败: {ex.Message}");
                        }
                    }
                });

                MessageBox.Show($"成功转换 {assFiles.Count} 个文件！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转换过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                BTN_Run.Enabled = true;
                BTN_Run.Text = "运行";
            }
        }

        private void GetAssFiles(string path)
        {
            assFiles.Clear();
            // 直接使用 SearchPattern 简化查找
            // SearchOption.TopDirectoryOnly 对应原本逻辑
            var files = Directory.GetFiles(path, "*.ass", SearchOption.TopDirectoryOnly);
            assFiles.AddRange(files);
        }

        private void ConvertAssToSrt(string assFilePath, string srtFilePath)
        {
            // 使用 Stream 读取，防止大文件内存占用过高，但在字幕文件通常很小，ReadAllLines 也无妨
            // 明确编码通常比较好，但从文件自动检测通常够用
            var lines = File.ReadAllLines(assFilePath);
            var srtLines = new List<string>();
            int index = 1;

            foreach (var line in lines)
            {
                // ASS 的 Dialogue 行通常以 "Dialogue:" 开头
                if (line.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase))
                {
                    // 1. 它是 CSV 格式，但 Text 字段可能包含逗号。
                    // 标准 ASS Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
                    // 前面有 9 个逗号，意味着分割成 10 部分。
                    // 我们限制 Split 最大为 10，这样第 10 部分就是完整的 Text (包含可能的逗号)

                    // 去掉 "Dialogue:" 前缀再分割，更加准确
                    string content = line.Substring(9).TrimStart();

                    // 注意：不能使用 RemoveEmptyEntries，因为 ASS 中 Style, Name, Effect 经常为空
                    // 限制 split 数量为 10，保证最后一个元素是文本
                    var parts = content.Split(new[] { ',' }, 10);

                    if (parts.Length >= 10)
                    {
                        // 索引: 0=Layer, 1=Start, 2=End, ..., 9=Text
                        string start = parts[1].Trim();
                        string end = parts[2].Trim();
                        string text = parts[9]; // 这里已经包含了原来文本里所有的逗号

                        // 清理 ASS 特效代码 {...}
                        text = Regex.Replace(text, @"\{[^}]*\}", string.Empty);
                        // 转换换行符 \N -> 实际换行
                        text = text.Replace(@"\N", Environment.NewLine).Trim();

                        // 忽略空行或特殊标记
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        string srtStart = ConvertAssTimeToSrtTime(start);
                        string srtEnd = ConvertAssTimeToSrtTime(end);

                        srtLines.Add(index.ToString());
                        srtLines.Add($"{srtStart} --> {srtEnd}");
                        srtLines.Add(text);
                        srtLines.Add(""); // SRT 字幕块之间需要空行
                        index++;
                    }
                }
            }

            File.WriteAllLines(srtFilePath, srtLines, Encoding.UTF8);
            Debug.WriteLine("转换完成：" + srtFilePath);
        }

        private string ConvertAssTimeToSrtTime(string assTime)
        {
            // ASS 时间格式: h:mm:ss.cc (例如 0:00:05.15) -> cc 是厘秒
            // SRT 时间格式: hh:mm:ss,fff (例如 00:00:05,150) -> fff 是毫秒

            var match = Regex.Match(assTime, @"(\d+):(\d+):(\d+)\.(\d+)");
            if (match.Success)
            {
                string hh = match.Groups[1].Value.PadLeft(2, '0');
                string mm = match.Groups[2].Value.PadLeft(2, '0');
                string ss = match.Groups[3].Value.PadLeft(2, '0');
                string cs = match.Groups[4].Value; // 厘秒

                // 处理厘秒转毫秒
                // ASS 的 2位厘秒 (0-99) 对应 毫秒的 (0-990)
                // 如果 assTime 是 1位小数 .5，代表 500ms
                // 如果 assTime 是 2位小数 .50，代表 500ms
                if (cs.Length == 1) cs += "00";
                else if (cs.Length == 2) cs += "0";

                // 确保不超过3位
                if (cs.Length > 3) cs = cs.Substring(0, 3);

                // 标准 SRT 使用逗号
                return $"{hh}:{mm}:{ss},{cs}";
            }

            // 如果解析失败，返回零时间以防崩溃
            return "00:00:00,000";
        }
    }
}