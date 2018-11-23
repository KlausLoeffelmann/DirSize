using ByteSizeLib;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

namespace DirSize
{
    // We need this to avoid flickering.
    // We patched the Designer Code to use this
    // instead of System.Windows.Forms.ListView.
    public class FolderListView : ListView
    {
        public FolderListView():base()
        {
            this.DoubleBuffered = true;
        }
    }

    public partial class MainForm : Form
    {
        private ToolStripButton _lastCheckedItem;
        private long _totalFolderBytes;
        private ActionBlock<(ListViewItem, Action<ByteSize>)> _workerBlock;
        private CancellationTokenSource _workerBlockCancellationTokenSource = new CancellationTokenSource();
        private DriveInfo _currentDriveInfo;

        public MainForm()
        {
            InitializeComponent();
            folderListView.View = View.Details;
            folderListView.GridLines = true;
            folderListView.HideSelection = false;
            folderListView.FullRowSelect = true;
            folderListView.Sorting = SortOrder.None;

            // It's not a real double click, but it'll do for our purposes.
            folderListView.Activation = ItemActivation.TwoClick;

            // Setting the width to -2 means the column width fits the Headlines.
            // This is just initially.
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

            this.folderListView.ListViewItemSorter = new ListViewItemComparer();
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
            EnsureWorkerBlockFinished();

            _currentDriveInfo = (DriveInfo)((ToolStripButton)sender).Tag;

            // Update StatusStrip
            diveLetterStatusLabel.Text = _currentDriveInfo.Name;
            freeSpaceStatusLabel.Text = $"{new ByteSize(_currentDriveInfo.TotalFreeSpace)} free on Drive.";
            totalBytesCapacityStatusLabel.Text = $"{new ByteSize(_currentDriveInfo.TotalSize)} total Drive size.";

            currentPathLabel.Text = _currentDriveInfo.Name;
            await ProcessFoldersAsync(_currentDriveInfo.Name, false);
        }

        private async void folderListView_ItemActivate(object sender, EventArgs e)
        {
            var ioItem = (SimpleIoItemInfoStructure)folderListView.SelectedItems[0].Tag;
            EnsureWorkerBlockFinished();
            currentPathLabel.Text = ioItem.Path;

            // Make sure, if we are up again via ..\.. ..., we do not crash.
            if (new DirectoryInfo(ioItem.Path).Parent == null)
            {
                await ProcessFoldersAsync(ioItem.Path, false);
            }
            else
            {
                await ProcessFoldersAsync(ioItem.Path, true);
            }
        }

        private void EnsureWorkerBlockFinished()
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
        }

        private async Task ProcessFoldersAsync(string path, bool addFolderUpItem)
        {
            var resultList = (from ioItem in IoEnumService.GetFilesAndDirs(path)
                              orderby ioItem.IsFolder descending,
                                      ioItem.Name
                              select ioItem).ToList();

            if (addFolderUpItem)
            {
                var listViewItem = new ListViewItem(new string[]{
                    "..",
                    "",
                    "",
                    "",
                    "" });

                listViewItem.Tag = new SimpleIoItemInfoStructure()
                {
                    Path = new DirectoryInfo(path).Parent.ToString(),
                    Name = "..",
                    IsFolder = true
                };
                folderListView.Items.Add(listViewItem);
            }

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

            // Setting the width to -1 means, we're fitting the widest entry width.
            for (int columnCount = 0; columnCount < 3; columnCount++)
            {
                folderListView.Columns[columnCount].Width = -1;
            }

            // Let's give the UI time to breath and update what's need to be updated.
            await Task.Delay(100);

            var totalFoldersProcessedUpdater = new Action<ByteSize>((additionallyUsedFolderBytes) =>
            {
                Interlocked.Add(ref _totalFolderBytes, (long)additionallyUsedFolderBytes.Bytes);
                statusStrip1.Invoke(new Action(() => InfoItemStatusLabel.Text = $"Retrieving folder sizes: {new ByteSize(_totalFolderBytes)}"));
            });

            var sortTriggerCounter = 0L;

            var blockAction = new Action<(ListViewItem lvItem, Action<ByteSize> updateAction)>((lvItemAndUpdater) =>
            {
                var ioItemTemp = (SimpleIoItemInfoStructure)lvItemAndUpdater.lvItem.Tag;
                if (ioItemTemp.IsFolder && ioItemTemp.Name != "..")
                {
                    var result = IoEnumService.GetFolderSizeRecursive(ioItemTemp.Path,
                                  (value) =>
                                  {
                                      // We print out the progress for each folder, while we're retrieving the data,
                                      // But only, if this is not the [..] Item to go up in the folder structure.
                                      folderListView.Invoke(
                                            new Action(() =>
                                            {
                                                var itemSum = ((long elements, ByteSize usedBytes))lvItemAndUpdater.lvItem.SubItems[3].Tag;
                                                itemSum.elements += value.additionalElementsCounted;
                                                itemSum.usedBytes += value.additionalBytesUsed;
                                                lvItemAndUpdater.lvItem.SubItems[3].Text = itemSum.usedBytes.ToString();
                                                lvItemAndUpdater.lvItem.SubItems[4].Text = itemSum.elements.ToString("#,##0");
                                                lvItemAndUpdater.lvItem.SubItems[3].Tag = itemSum;
                                                lvItemAndUpdater.updateAction.Invoke(value.additionalBytesUsed);

                                                if (sortTriggerCounter++ % 100 == 0)
                                                {
                                                    folderListView.Sort();
                                                }
                                            }));
                                  });

                    // We print out the result, once it's available.
                    folderListView.Invoke(
                            new Action(() =>
                            {
                                    // We need to update the Tag, because we need the final result here for sorting purposes
                                    // for the Folder Sizes which are still gathered on other threads.
                                    lvItemAndUpdater.lvItem.SubItems[3].Tag = result;
                                lvItemAndUpdater.lvItem.SubItems[3].Text = result.totalBytesUsed.ToString();
                                lvItemAndUpdater.lvItem.SubItems[4].Text = result.totalElements.ToString("#,##0");

                                // And we need the final sort.
                                folderListView.Sort();
                            }));
                }
            });

            _workerBlock = new ActionBlock<(ListViewItem, Action<ByteSize>)>(blockAction,
                                            new ExecutionDataflowBlockOptions()
                                            {
                                                MaxDegreeOfParallelism = Environment.ProcessorCount / 2,
                                                CancellationToken = _workerBlockCancellationTokenSource.Token
                                            });


            foreach (ListViewItem lvItem in folderListView.Items)
            {
                var status = _workerBlock.Post((lvItem, totalFoldersProcessedUpdater));
            }

            _workerBlock.Complete();

            try
            {
                await _workerBlock.Completion;
                statusStrip1.Invoke(new Action(() => InfoItemStatusLabel.Text = $"Done."));
            }
            catch (TaskCanceledException)
            {
                statusStrip1.Invoke(new Action(() => InfoItemStatusLabel.Text = $"Canceled."));
            }
        }
    }

    class ListViewItemComparer : IComparer
    {
        private int col;
        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            try
            {
                // null test is too expensive. Let's rather fail in that case and return 0.
                var itemSum1 = ((long elements, ByteSize usedBytes))((ListViewItem)x).SubItems[3].Tag;
                var itemSum2 = ((long elements, ByteSize usedBytes))((ListViewItem)y).SubItems[3].Tag;
                return itemSum2.usedBytes.Bytes.CompareTo(itemSum1.usedBytes.Bytes);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
