﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DataTableToExcel;

namespace SpectroChipApp
{
    public partial class Form4 : Form
    {
        DataTable table = new DataTable();
        private Form1 f1;

        public Form4(Form1 form)
        {
            InitializeComponent();
            f1 = form;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderPath = new FolderBrowserDialog();
            FolderPath.ShowDialog();
            textBox1.Text = FolderPath.SelectedPath;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            // Create new DataTable.
            //DataTable table = new DataTable();

            // Declare DataColumn and DataRow variables.
            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, ColumnName
            // and add to DataTable.    
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "波長";
            table.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "強度";

            table.Columns.Add(column);

            // Create new DataRow objects and add to DataTable.    
            for (int x = 0; x < f1.W; x++)
            {
                row = table.NewRow();
                row["波長"] = x + f1.X_Start;
                row["強度"] = f1.IntensitySG[x];
                table.Rows.Add(row);
            }
            DataTableToExcel.DataTableToExcel.ExportToExcel(@"" + textBox1.Text + @"\" + textBox2.Text + ".xlsx", "sheet123", table);
            MessageBox.Show("存檔完成");
            this.Close();
        }
    }
}
