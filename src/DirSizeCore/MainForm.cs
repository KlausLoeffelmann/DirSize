using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    }
}
