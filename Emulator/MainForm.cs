using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emulator
{
    public partial class MainForm : Form
    {
        private CPU _cpu;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDia.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDia.Filter = "Binary Files|*.bin";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDia.ShowDialog() != DialogResult.OK)
                return;

            var bytes = System.IO.File.ReadAllBytes(openFileDia.FileName);
            if (bytes == null || bytes.Length > 128)
            {
                MessageBox.Show("Program length can't be longer than 128 bytes", "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CPUMemory memory = new CPUMemory(bytes);
            _cpu = new CPU(memory);

            RefreshData();
        }

        private void RefreshData()
        {
            RefreshRegisters();
            RefreshFlags();
            RefreshRom();
            RefreshRam();
        }

        private void RefreshRam()
        {
            if (_cpu == null) return;

            gridRam.Rows.Clear();

            for (byte i = 128; i <= 255; i++)
            {
                var mem = _cpu.Memory.Read(i);
                gridRam.Rows.Add(i.ToString(), Convert.ToString(mem, 16), mem);

                if (i == 255) break;
            }
        }

        private void RefreshRom()
        {
            if (_cpu == null) return;

            gridRom.Rows.Clear();

            for (byte i = 0; i < 128; i++)
            {
                var mem = _cpu.Memory.Read(i);
                gridRom.Rows.Add(i.ToString(), Convert.ToString(mem, 16), mem);
            }
        }

        private void RefreshFlags()
        {
            if (_cpu == null) return;

            chkCF.Checked = _cpu.CF;
            chkSF.Checked = _cpu.SF;
            chkZF.Checked = _cpu.ZF;
            chkPF.Checked = _cpu.PF;
        }

        private void RefreshRegisters()
        {
            if (_cpu == null) return;

            txtAX.Text = Convert.ToString(_cpu.AX, 16);
            txtBX.Text = Convert.ToString(_cpu.BX, 16);
            txtCX.Text = Convert.ToString(_cpu.CX, 16);
            txtDX.Text = Convert.ToString(_cpu.DX, 16);

            txtACC.Text = Convert.ToString(_cpu.ACC, 16);

            txtPC.Text = Convert.ToString(_cpu.PC, 16);
            txtCT.Text = Convert.ToString(_cpu.CT, 16);
            txtIR.Text = Convert.ToString(_cpu.IR, 16);
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            _cpu.RunInstruction();
            RefreshData();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _cpu.Reset();
            RefreshData();
        }

        private void gridRom_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            byte address = (byte)(e.RowIndex);
            MemoryChangerForm form = new MemoryChangerForm(_cpu.Memory, address);
            if (form.ShowDialog() == DialogResult.OK)
                RefreshData();
        }

        private void gridRam_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            byte address = (byte)(e.RowIndex + 128);
            MemoryChangerForm form = new MemoryChangerForm(_cpu.Memory, address);
            if (form.ShowDialog() == DialogResult.OK)
                RefreshData();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
 