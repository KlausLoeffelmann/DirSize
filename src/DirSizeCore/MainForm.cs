using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirSize
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            folderListView.View = View.Details;
            folderListView.GridLines = true;
            folderListView.HideSelection = false;
            folderListView.FullRowSelect = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //var circle = Image.FromFile("resource\\circle.wmf");
            var buttonFont = new System.Drawing.Font("Segoe UI", 12F,
                                                     System.Drawing.FontStyle.Bold,
                                                     System.Drawing.GraphicsUnit.Point);

            foreach (var driveItem in IoEnumService.GetDrives())
            {
                var first = false;
                var tsButton = new ToolStripButton
                {
                    Font = buttonFont,
                    ImageTransparentColor = System.Drawing.Color.Magenta,
                    Name = "tsButtonDrive" + driveItem.Name.Replace("\\", ""),
                    Size = new System.Drawing.Size(64, 64),
                    Text = driveItem.Name,
                    TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay,
                    CheckOnClick = true
                };
                tsButton.CheckedChanged += new EventHandler(this.DriveButton_Click);
                tsButton.Tag = driveItem;
                if (!first)
                {
                    first = true;
                    toolStrip1.BeginInvoke(new Action(() => tsButton.CheckState = CheckState.Checked));
                }
                toolStrip1.Items.Add(tsButton);
            }
        }

        public IIoEnumService IoEnumService { get; set; }

        private async void DriveButton_Click(object sender, EventArgs e)
        {
            Action toogleButtons = new Action(() =>
              {
                  foreach (var item in toolStrip1.Items)
                  {
                      if ((item is ToolStripButton tsItem))
                      {
                          tsItem.Enabled = tsItem.Enabled ^ tsItem.Tag != null;
                      }
                  }
              });

            var resultList = IoEnumService.GetFilesAndDirs(((DriveInfo)((ToolStripButton)sender).Tag).Name).ToList();
            foreach (var item in resultList)
            {
                folderListView.Items.Add(item.Path);
            }
            await Task.Delay(0);
        }
    }
}
