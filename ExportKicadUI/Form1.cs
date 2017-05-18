using Rop.Kicad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExportKicadUI
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private async void button3_Click(object sender, EventArgs e)
		{
			button3.Enabled = false;
			var d = new Rop.Kicad.PcbFileParser();
			var a = textBox1.Text;
			var r = d.Parse(a);
			await Task.Run(() =>
			{
				var c = new CorelDrawPaint();
				c.SkipDrillSize = checkBox1.Checked;
				c.MarkBigDrill = checkBox1.Checked;
				c.DrawPcb(r);

			});
			button3.Enabled = true;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox1.Text = openFileDialog1.FileName;
			}
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{

		}
	}
}
