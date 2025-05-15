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

        private void CarregarChamadoNoControle(int idChamado)
        {
            SqlConnection conn = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=SuporteChamados;Integrated Security=True");
            SqlCommand cmd = new SqlCommand(@"
        SELECT 
            c.idChamado,
            cli.nomeCliente,
            c.assunto,
            c.dataAbertura,
            c.horaAbertura
        FROM tblChamados c
        INNER JOIN tblClientes cli ON cli.idCliente = c.idCliente
        WHERE c.idChamado = @idChamado", conn);

            cmd.Parameters.AddWithValue("@idChamado", idChamado);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Cria o controle
                Chamados controle = new Chamados();

                // Preenche os dados do chamado no controle
                controle.IdChamado = Convert.ToInt32(reader["idChamado"]);
                controle.Assunto = reader["assunto"].ToString();
                controle.Cliente = reader["nomeCliente"].ToString();
                controle.DataAbertura = Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy");
                controle.HoraAbertura = TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm");

                // Mostra o controle na interface (exemplo: painelConteudo é um Panel)
                flowLayoutPanel1.Controls.Clear();
                flowLayoutPanel1.Controls.Add(controle);
            }
            else
            {
                MessageBox.Show("Chamado não encontrado.");
            }
            conn.Close();
        }

        private void telaPrincipal_Load(object sender, EventArgs e)
        {

            CarregarTodosChamados();
        }

        private void CarregarTodosChamados()
        {
            flowLayoutPanel1.Controls.Clear();

            SqlConnection conn = new SqlConnection(strconexao);
            SqlCommand cmd = new SqlCommand(@"
        SELECT 
            c.idChamado,
            cli.nomeCliente,
            c.assunto,
            c.dataAbertura,
            c.horaAbertura
        FROM tblChamados c
        INNER JOIN tblClientes cli ON cli.idCliente = c.idCliente", conn);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Chamados controle = new Chamados();
                controle.IdChamado = Convert.ToInt32(reader["idChamado"]);
                controle.Assunto = reader["assunto"].ToString();
                controle.Cliente = reader["nomeCliente"].ToString();
                controle.DataAbertura = Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy");
                controle.HoraAbertura = TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm");

                flowLayoutPanel1.Controls.Add(controle);
            }

            conn.Close();
        }

        private void pnLista_Paint(object sender, PaintEventArgs e)
        {
            CarregarTodosChamados();
        }
    }
}
