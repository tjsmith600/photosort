using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSort
{
    public partial class frmMain : Form
    {
        PhotoSort photoSort = new PhotoSort();

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            string folderPath = GetFolderName(txtSourceFolder.Text);

            if (folderPath != null)
                txtSourceFolder.Text = folderPath;
        }

        private void btnBrowseDestination_Click(object sender, EventArgs e)
        {
            string folderPath = GetFolderName(txtDestinationFolder.Text);

            if (folderPath != null)
                txtDestinationFolder.Text = folderPath;
        }

        private string GetFolderName(string folderPath)
        {
            string path = null;
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                if (String.IsNullOrEmpty(folderPath) == false)
                    dialog.InitialDirectory = folderPath;
                
                dialog.IsFolderPicker = true;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    path = dialog.FileName;
                }
            }
            return path;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // save settings when closing
            Properties.Settings.Default["SourceFolder"] = txtSourceFolder.Text;
            Properties.Settings.Default["DestinationFolder"] = txtDestinationFolder.Text;

            Properties.Settings.Default.Save();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            txtSourceFolder.Text = Properties.Settings.Default["SourceFolder"].ToString();
            txtDestinationFolder.Text = Properties.Settings.Default["DestinationFolder"].ToString();
        }

        private async void btnParseFiles_Click(object sender, EventArgs e)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/handling-reentrancy-in-async-apps

            SetControlsEnabled(false);

            long megabytesPerDisc = Convert.ToInt64(4.5 * 1024);

            try
            {
                await Task.Run(() => photoSort.ParseFiles(txtSourceFolder.Text, txtDestinationFolder.Text, megabytesPerDisc * 1024 * 1024));
            }
            catch (Exception)
            {
                // not sure what to do with these yet
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnBrowseDestination.Enabled = enabled;
            btnBrowseSource.Enabled = enabled;
            btnParseFiles.Enabled = enabled;

            txtDestinationFolder.Enabled = enabled;
            txtSourceFolder.Enabled = enabled;
        }
    }
}
