using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prototipo_Suport
{
    public partial class TelaDeChamadocs : Form
    {
        public TelaDeChamadocs()
        {
            InitializeComponent();
        }

        private void TelaDeChamadocs_Load(object sender, EventArgs e)
        {
            //AbrirTelaChamadocs();
        }
        private void AbrirTelaChamadocs() // ou em um botão, por exemplo
        {
            TelaDeChamadocs chamadocs = new TelaDeChamadocs();
            this.Hide();
            chamadocs.FormClosed += (s, args) => this.Show();
            chamadocs.Show();
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            telaPrincipal paginaPrincipal = new telaPrincipal();
            paginaPrincipal.Show();
            this.Close(); // ou this.Hide(); se quiser manter esta tela na memória
        }
    }
}
