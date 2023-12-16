using System.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXLinkFormatter {
    public partial class FormClipboardRecords : Form {
        private List<ClipboardRecord> records = new List<ClipboardRecord>();
        private bool skipAdding = false;

        public FormClipboardRecords() {
            InitializeComponent();

            this.FormClosing += FormClipboardRecords_FormClosing;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowHeadersWidth = 30;

            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = col.Name = "Content";
            col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            col.Width = 1000;
            dataGridView1.Columns.Add(col);

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void FormClipboardRecords_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void AddNewRecord(string content, ClipboardContentType type) {
            if (skipAdding || string.IsNullOrEmpty(content))
                return;
            
            var newRecord = new ClipboardRecord();

            newRecord.Content = content;
            newRecord.ContentType = type;
            newRecord.Id = records.Count;

            records.Insert(0, newRecord);
            dataGridView1.DataSource = new BindingList<ClipboardRecord>(records);
            //dataGridView1.AutoResizeColumn(0, DataGridViewAutoSizeColumnMode.DisplayedCells);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex == -1)
                return;

            var record = dataGridView1.Rows[e.RowIndex].Cells["Content"].Value.ToString();

            skipAdding = true;
            Clipboard.SetText(record);
            setTimeout<int>(param => {
                skipAdding = false;
            }, 1, this, 200);
        }

        public void setTimeout<T>(Action<T> action, T param, Control control, int timeout) {
            var thread = new Thread(() => {
                Thread.Sleep(timeout);
                control.BeginInvoke(action, param);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}