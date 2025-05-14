using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prototipo_Suport
{
    public partial class telaPrincipal : Form
    {
        string strconexao = "Data Source =.\\SQLEXPRESS;Initial Catalog = SuporteChamados; Integrated Security = True";
        SqlConnection objconexao;
        public telaPrincipal()
        {
            InitializeComponent();
        }

        private void btnNovoChamado_Click(object sender, EventArgs e)
        {
            TelaDeChamadocs telaDeChamadocs = new TelaDeChamadocs();
            telaDeChamadocs.Show();
            this.Hide();
            
        }
    }
}
