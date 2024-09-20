using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using ControlGallery;
using Modern.Forms;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;

namespace SimpleFileSearch
{
    public partial class MainWindowMF : Form
    {
        // Controls
        private OpenFileDialog openFileDialog1;
        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label2;
        private Panel panel1;
        private Label lblEstado;
        private CheckBox chkCaseSens;
        private Button btnSearchFolder;
        private ComboBox cmbFileName;
        private Label label3;
        private ComboBox cmbPath;
        private ComboBox cmbInFile;
        private Label label1;
        private Button btnSearch;
        private Button btnCancel;
        private TableLayoutPanel tableLayoutPanel2;
        private ListBox lstFiles;
        private Label lblStatus;

        public MainWindowMF()
        {
            InitializeComponent();
            LoadData();
            this.Text += " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        private void InitializeComponent()
        {
            // Initialize components for Modern.Forms
            openFileDialog1 = new OpenFileDialog();

            // MainWindow properties
            Text = "GH Software File Search";
            Size = new Size(1318, 825);
            StartPosition = FormStartPosition.CenterScreen;

            // SplitContainer setup
            splitContainer1 = new SplitContainer
            {
                Orientation = Orientation.Horizontal,
                Dock = DockStyle.Fill,
                Panel1MinimumSize = 320,
                Panel2MinimumSize = 120,
                SplitterColor = SKColors.Gray
            };
            if (Settings.Current.SplitContainer1Panel1Width > 0)
            {
                splitContainer1.Panel1.Width = Settings.Current.SplitContainer1Panel1Width;
                splitContainer1.Panel2.Width = Settings.Current.SplitContainer1Panel2Width;
            }
            else
            {
                splitContainer1.Panel1.Width = splitContainer1.Panel2.Width = splitContainer1.Width / 2;
            }

            splitContainer1.Panel1.SizeChanged += this.SplitContainer1_SizeChanged;
            splitContainer1.Panel2.SizeChanged += this.SplitContainer1_SizeChanged;
            Controls.Add(splitContainer1);

            // Left Panel (Panel1)
            tableLayoutPanel1 = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 9,
                Dock = DockStyle.Fill
            };
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);

            // Column and Row Styles
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            for (int i = 0; i < 9; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tableLayoutPanel1.RowStyles[8] = new RowStyle(SizeType.Percent, 100F);


            // Controls on Left Panel
            label2 = new Label { Text = "File name", AutoSize = true };
            tableLayoutPanel1.Controls.Add(label2, 0, 0);

            cmbFileName = new ComboBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            tableLayoutPanel1.Controls.Add(cmbFileName, 0, 1);
            tableLayoutPanel1.SetColumnSpan(cmbFileName, 3);

            label3 = new Label { Text = "Search in files contents", AutoSize = true };
            tableLayoutPanel1.Controls.Add(label3, 0, 2);

            cmbInFile = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            tableLayoutPanel1.Controls.Add(cmbInFile, 0, 3);
            tableLayoutPanel1.SetColumnSpan(cmbInFile, 3);

            chkCaseSens = new CheckBox { Text = "Case sensitive", Anchor = AnchorStyles.Left | AnchorStyles.Right };
            chkCaseSens.Style.BackgroundColor = SkiaSharp.SKColors.Transparent;
            tableLayoutPanel1.Controls.Add(chkCaseSens, 0, 4);
            tableLayoutPanel1.SetColumnSpan(chkCaseSens, 3);

            label1 = new Label { Text = "Path", AutoSize = true };
            tableLayoutPanel1.Controls.Add(label1, 0, 5);

            cmbPath = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            tableLayoutPanel1.Controls.Add(cmbPath, 0, 6);
            tableLayoutPanel1.SetColumnSpan(cmbPath, 2);

            btnSearchFolder = new Button { Text = "..." };
            tableLayoutPanel1.Controls.Add(btnSearchFolder, 2, 6);
            btnSearchFolder.Click += btnSearchFolder_Click;

            btnSearch = new Button { Text = "Search" };
            tableLayoutPanel1.Controls.Add(btnSearch, 0, 7);
            btnSearch.Click += btnSearch_Click;

            btnCancel = new Button { Text = "Close" };
            tableLayoutPanel1.Controls.Add(btnCancel, 1, 7);
            btnCancel.Click += BtnCancel_Click;

            panel1 = new Panel { Dock = DockStyle.Fill };
            lblEstado = new Label { AutoSize = true };
            panel1.Controls.Add(lblEstado);
            tableLayoutPanel1.Controls.Add(panel1, 0, 8);
            tableLayoutPanel1.SetColumnSpan(panel1, 3);

            // Right Panel (Panel2)
            tableLayoutPanel2 = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill
            };
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel2);

            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            lblStatus = new Label
            {
                Text = "Website",
                AutoSize = true,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right
            };
            lblStatus.Click += lblStatus_Click;

            tableLayoutPanel2.Controls.Add(lblStatus, 1, 0);

            lstFiles = new ListBox { Dock = DockStyle.Fill };
            lstFiles.DoubleClick += lstFiles_DoubleClick;
            tableLayoutPanel2.Controls.Add(lstFiles, 0, 1);
            tableLayoutPanel2.SetColumnSpan(lstFiles, 2);
        }

        private void SplitContainer1_SizeChanged(object sender, EventArgs e)
        {
            Settings.Current.SplitContainer1Panel1Width = splitContainer1.Panel1.Width;
            Settings.Current.SplitContainer1Panel2Width = splitContainer1.Panel2.Width;
            Settings.Save();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveData()
        {
            const int tammax = 50;

            if (!Settings.Current.FileNameHistory.Contains(cmbFileName.Text))
            {
                Settings.Current.FileNameHistory.Insert(0, cmbFileName.Text);
                cmbFileName.Items.Insert(0, cmbFileName.Text);
                if (Settings.Current.FileNameHistory.Count > tammax)
                {
                    Settings.Current.FileNameHistory.RemoveAt(tammax - 1);
                }
            }

            if (!Settings.Current.SearchInsideFiles.Contains(cmbInFile.Text))
            {
                Settings.Current.SearchInsideFiles.Insert(0, cmbInFile.Text);
                cmbInFile.Items.Insert(0, cmbInFile.Text);
                if (Settings.Current.SearchInsideFiles.Count > tammax)
                {
                    Settings.Current.SearchInsideFiles.RemoveAt(tammax - 1);
                }
            }
            if (!Settings.Current.PathHistory.Contains(cmbPath.Text))
            {
                Settings.Current.PathHistory.Insert(0, cmbPath.Text);
                cmbPath.Items.Insert(0, cmbPath.Text);
                if (Settings.Current.PathHistory.Count > tammax)
                {
                    Settings.Current.PathHistory.RemoveAt(tammax - 1);
                }
            }

            Settings.Save();
        }

        private void LoadData()
        {
            foreach (string s in Settings.Current.FileNameHistory)
            {
                cmbFileName.Items.Add(s);
            }

            foreach (string s in Settings.Current.SearchInsideFiles)
            {
                cmbInFile.Items.Add(s);
            }
            foreach (string s in Settings.Current.PathHistory)
            {
                if (string.IsNullOrEmpty(s))
                    continue; cmbPath.Items.Add(s);
            }

            cmbPath.Text = Settings.Current.CurrentDirectory;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            PrintStatus("Saving data...");
            SaveData();
            lstFiles.Items.Clear();
            PrintStatus("Searching files...");
            FileSearcher fs = new FileSearcher();
            fs.estado += fs_estado;

            List<FileInfo> files = fs.SearchFiles(cmbPath.Text, cmbFileName.Text, cmbInFile.Text, chkCaseSens.Checked);

            PrintStatus("Listing files...");

            int limit = 2000;
            foreach (FileInfo f in files)
            {
                lstFiles.Items.Add(f);
                limit--;
                if (limit < 0)
                    break;
            }

            PrintStatus("Ready.");
        }

        void fs_estado(string value)
        {
            PrintStatus(value);
        }

        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItem != null)
            {
                FileInfo f = (FileInfo)lstFiles.SelectedItem;
                string directoryPath = f.Directory.FullName;
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Process.Start("explorer.exe", directoryPath);
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        Process.Start("open", directoryPath);
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        Process.Start("xdg-open", directoryPath);
                    }
                }
                catch (Exception ex)
                {
                    new DialogForm($"Failed to open directory: {ex.Message}").ShowDialog(this);
                }
            }
        }

        private void PrintStatus(string status)
        {
            lblEstado.Text = status;
            // Application.DoEvents(); // Not available in Modern.Forms
            // lblEstado.Refresh();
        }



        private void lblStatus_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/GustavoHennig/FileSearch",
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
            }
        }

        private async void btnSearchFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = cmbPath.Text;
            if ((await f.ShowDialog(this)) == DialogResult.OK)
            {
                cmbPath.Text = f.SelectedPath;
            }
        }


        private void label2_Click(object sender, EventArgs e)
        {
            MainWindowMF mainWindowMF = new MainWindowMF();
            mainWindowMF.Show();
        }


        private void ProcessStart(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", path);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", path);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", path);
            }


        }
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {

            }
        }
    }
}