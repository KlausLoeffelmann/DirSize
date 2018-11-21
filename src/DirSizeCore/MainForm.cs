using ByteSizeLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

namespace DirSize
{
    public partial class MainForm : Form
    {
        private ToolStripButton _lastCheckedItem;
        private long _totalFolderBytes;
        private ActionBlock<(ListViewItem, Action<ByteSize>)> _workerBlock;
        private CancellationTokenSource _workerBlockCancellationTokenSource = new CancellationTokenSource();

        public MainForm()
        {
            InitializeComponent();
            folderListView.View = View.Details;
            folderListView.GridLines = true;
            folderListView.HideSelection = false;
            folderListView.FullRowSelect = true;

            folderListView.Columns.Add(new ColumnHeader()
            {
                Text = "Name",
                Width = -2,
                TextAlign = HorizontalAlignment.Left
            });

            folderListView.Columns.Add(new ColumnHeader()
            {
                Text = "Date modified",
                Width = -2,
                TextAlign = HorizontalAlignment.Left
            });

            folderListView.Columns.Add(new ColumnHeader()
            {
                Text = "Type",
                Width = -2,
                TextAlign = HorizontalAlignment.Left
            });

            folderListView.Columns.Add(new ColumnHeader()
            {
                Text = "Size",
                Width = 100,
                TextAlign = HorizontalAlignment.Left
            });

            folderListView.Columns.Add(new ColumnHeader()
            {
                Text = "File count",
                Width = -2,
                TextAlign = HorizontalAlignment.Left
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //var circle = Image.FromFile("resource\\circle.wmf");
            var buttonFont = new System.Drawing.Font("Segoe UI", 12F,
                                                     System.Drawing.FontStyle.Bold,
                                                     System.Drawing.GraphicsUnit.Point);

            var first = false;

            foreach (var driveItem in IoEnumService.GetDrives())
            {
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
                tsButton.Click += new EventHandler(this.DriveButton_Click);
                tsButton.CheckedChanged += new EventHandler(this.DriveButton_CheckChanged);
                tsButton.Tag = driveItem;
                if (!first)
                {
                    first = true;
                    tsButton.PerformClick();
                }
                toolStrip1.Items.Add(tsButton);
            }
        }

        public IIoEnumService IoEnumService { get; set; }

        private void DriveButton_CheckChanged(object sender, EventArgs e)
        {
            if (_lastCheckedItem!=null)
            {
                _lastCheckedItem.CheckState = CheckState.Unchecked;
            }
            _lastCheckedItem = sender as ToolStripButton;
        }

        private async void DriveButton_Click(object sender, EventArgs e)
        {
            // If we are in the middle of retrieving the folder sizes for the current list...
            if (_workerBlock != null && !_workerBlock.Completion.IsCompleted)
            {
                // ...let's cancel all those running tasks.
                _workerBlockCancellationTokenSource.Cancel();
                _workerBlockCancellationTokenSource = new CancellationTokenSource();
            }

            folderListView.Items.Clear();
            _totalFolderBytes = 0;
            var driveInfo = (DriveInfo)((ToolStripButton)sender).Tag;

            // Update StatusStrip
            diveLetterStatusLabel.Text = driveInfo.Name;
            freeSpaceStatusLabel.Text = $"{new ByteSize(driveInfo.TotalFreeSpace)} free on Drive.";
            totalBytesCapacityStatusLabel.Text = $"{new ByteSize(driveInfo.TotalSize)} total Drive size.";

            var resultList = (from ioItem in IoEnumService.GetFilesAndDirs(((DriveInfo)((ToolStripButton)sender).Tag).Name)
                              orderby ioItem.IsFolder descending,
                                      ioItem.Name
                              select ioItem).ToList();

            foreach (var item in resultList)
            {
                var listViewItem = new ListViewItem(new string[]{
                    item.Name,
                    item.DateModified.ToString("yyyy-mm-dd HH:mm:ss"),
                    item.IsFolder ? "File Folder" : "File",
                    item.Size.HasValue ?
                    item.Size.Value.ToString("#,##0") :
                        "- - -",
                    item.Count.HasValue?
                        item.Count.Value.ToString("#,##0") :
                        "- - -"});

                    listViewItem.Tag = item;

                // Make sure, every SubItem.Tag contains of a initialized Tuple(Long, Long),
                // so we do not need to test for null when casting it back from Tag.
                foreach (ListViewItem.ListViewSubItem subItem in listViewItem.SubItems)
                {
                    subItem.Tag = (0L, new ByteSize(0));
                }

                folderListView.Items.Add(listViewItem);
            }

            for (int columnCount = 0; columnCount < 3; columnCount++)
            {
                folderListView.Columns[columnCount].Width = -1;
            }

            var totalFoldersProcessedUpdater = new Action<ByteSize>((additionallyUsedFolderBytes) =>
            {
                Interlocked.Add(ref _totalFolderBytes, (long) additionallyUsedFolderBytes.Bytes);
                statusStrip1.BeginInvoke(new Action(() => InfoItemStatusLabel.Text = $"Retrieving folder sizes: {new ByteSize(_totalFolderBytes)}"));
            });

            var blockAction = new Action<(ListViewItem lvItem, Action<ByteSize> updateAction)>((lvItemAndUpdater) =>
              {
                  var ioItemTemp = (SimpleIoItemInfoStructure)lvItemAndUpdater.lvItem.Tag;
                  if (ioItemTemp.IsFolder)
                  {
                      var result = IoEnumService.GetFolderSizeRecursive(ioItemTemp.Path,
                                    (value) =>
                                    {
                                        // We print out the progress for each folder, while we're retrieving the data.
                                        folderListView.BeginInvoke(
                                            new Action(() =>
                                            {
                                                var itemSum = ((long elements, ByteSize usedBytes))lvItemAndUpdater.lvItem.SubItems[3].Tag;
                                                itemSum.elements += value.additionalElementsCounted;
                                                itemSum.usedBytes += value.additionalBytesUsed;
                                                lvItemAndUpdater.lvItem.SubItems[3].Text = itemSum.usedBytes.ToString();
                                                lvItemAndUpdater.lvItem.SubItems[4].Text = itemSum.elements.ToString("#,##0");
                                                lvItemAndUpdater.lvItem.SubItems[3].Tag = itemSum;
                                                lvItemAndUpdater.updateAction.Invoke(value.additionalBytesUsed);
                                            }));
                                    });

                      // We print out the result, once it's available.
                      folderListView.BeginInvoke(
                          new Action(() =>
                          {
                              lvItemAndUpdater.lvItem.SubItems[3].Text = result.totalBytesUsed.ToString();
                              lvItemAndUpdater.lvItem.SubItems[4].Text = result.totalElements.ToString("#,##0");
                          }));
                  }
              });

            _workerBlock = new ActionBlock<(ListViewItem, Action<ByteSize>)>(blockAction,
                                            new ExecutionDataflowBlockOptions()
                                            { MaxDegreeOfParallelism = 4,
                                              CancellationToken=_workerBlockCancellationTokenSource.Token});

            foreach (ListViewItem lvItem in folderListView.Items)
            {
                var status = await _workerBlock.SendAsync((lvItem, totalFoldersProcessedUpdater));
            }

            _workerBlock.Complete();
            try
            {
                await _workerBlock.Completion;
                statusStrip1.BeginInvoke(new Action(() => InfoItemStatusLabel.Text = $"Done."));
            }
            catch (TaskCanceledException)
            {
                statusStrip1.BeginInvoke(new Action(() => InfoItemStatusLabel.Text = $"Canceled."));
            }
        }
    }
}
