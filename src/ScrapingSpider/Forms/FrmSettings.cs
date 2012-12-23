using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using ScrapingSpider.Core.Models;

namespace ScrapingSpider.Forms
{
    public partial class FrmSettings : Form
    {
        public Settings Settings { get; set; }

        public FrmSettings()
        {
            InitializeComponent();
        }

        private void FrmSettings_Load(object sender, EventArgs e)
        {
            Settings = Properties.Settings.Default.SpiderSettings ?? new Settings();
            settingsBindingSource.DataSource = Settings;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.settingsBindingSource.EndEdit();
            Properties.Settings.Default.SpiderSettings = Settings;
            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
        }

        private void FrmSettings_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show("参数说明：" + Environment.NewLine +
                            "Init Seeds: 初始Url地址，多个地址使用回车分开。" + Environment.NewLine +
                            "Keyword: 关键字，按照指定的关键字爬取，多个关键字使用回车分开。" + Environment.NewLine +
                            "Crawl Depth: 爬取深度，小于0表示不限" + Environment.NewLine +
                            "Escape Links: 要过滤的链接，如：.jpg|.rar|.exe" + Environment.NewLine +
                            "Keep Cookie: 抓取过程中是否保留Cookie" + Environment.NewLine +
                            "Lock Host: 是否锁定Host，锁定后，指抓取站点相关链接。" + Environment.NewLine +
                            "Limit Speed: 是否智能限速。" + Environment.NewLine +
                            "Threads: 线程数量，启用多个线程有利于提高爬取效率。" + Environment.NewLine +
                            "Timeout: 超时时间，以毫秒为单位。" + Environment.NewLine +
                            "User Agent: http协议UserAgent设置。",
                "使用帮助 - Created By: StanZhai", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
            e.Cancel = true;
        }
    }
}
