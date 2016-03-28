using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emulator
{
    public partial class MemoryChangerForm : Form
    {
        private CPUMemory _memory;
        private byte _address;

        public MemoryChangerForm(CPUMemory memory, byte address)
        {
            InitializeComponent();

            _memory = memory;
            _address = address;
        }

        private void MemoryChangerForm_Load(object sender, EventArgs e)
        {
            txtAddress.Text = _address.ToString();
            txtOld.Text = Convert.ToString(_memory.Read(_address), 16).ToUpperInvariant() + "H";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var txt = txtNew.Text.Trim();
            if (string.IsNullOrWhiteSpace(txt))
            {
                ShowError();
                return;
            }

            try
            {
                var newVal = Convert.ToByte(txt, 16);
                _memory.ForceWrite(_address, newVal);

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.Message);
                ShowError();
            }
        }

        private static void ShowError()
        {
            MessageBox.Show("Enter a valid value or press cancel", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
