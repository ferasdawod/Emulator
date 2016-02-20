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
    public partial class Form1 : Form
    {
        private CPU _cpu;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _cpu = new CPU();

            openFileDia.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDia.Filter = "Binary Files|*.bin";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDia.ShowDialog() == DialogResult.OK)
            {
                var bytes = System.IO.File.ReadAllBytes(openFileDia.FileName);
                if (bytes.Length > 128)
                    MessageBox.Show("The program can't be larger than 128 bytes");

                _cpu.LoadRomMemory(bytes);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _cpu.Start();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _cpu.Pause();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cpu.Stop();
        }
    }

    class Student
    {
        int age;
        string name;
        float avarage;
    }
}
