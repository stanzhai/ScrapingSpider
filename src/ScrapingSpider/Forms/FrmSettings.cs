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
    }
}
