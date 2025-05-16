using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Prototipo_Suport
{
    public partial class telaPrincipal : Form
    {
        string strconexao = "Data Source=.\\SQLEXPRESS;Initial Catalog=SuporteChamados;Integrated Security=True";

        public telaPrincipal()
        {
            InitializeComponent();
        }

        private void btnNovoChamado_Click(object sender, EventArgs e)
        {
            // Exemplo de ID do chamado. Se você tiver uma seleção, substitua por esse ID
            int idChamadoGerado = 1; // Exemplo de ID de um chamado específico
            TelaDeChamadocs telaDeChamadocs = new TelaDeChamadocs(idChamadoGerado); // Passando o ID do chamado
            telaDeChamadocs.Show();
            this.Hide();
        }

        private void CarregarChamadoNoControle(int idChamado)
        {
            using (SqlConnection conn = new SqlConnection(strconexao))
            {
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
                    Chamados controle = new Chamados
                    {
                        IdChamado = Convert.ToInt32(reader["idChamado"]),
                        Assunto = reader["assunto"].ToString(),
                        Cliente = reader["nomeCliente"].ToString(),
                        DataAbertura = Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy"),
                        HoraAbertura = TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm")
                    };

                    flowLayoutPanel1.Controls.Clear();
                    flowLayoutPanel1.Controls.Add(controle);
                }
                else
                {
                    MessageBox.Show("Chamado não encontrado.");
                }
            }
        }

        private void telaPrincipal_Load(object sender, EventArgs e)
        {
            CarregarTodosChamados();
        }

        private void CarregarTodosChamados()
        {
            flowLayoutPanel1.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(strconexao))
            {
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
                    Chamados controle = new Chamados
                    {
                        IdChamado = Convert.ToInt32(reader["idChamado"]),
                        Assunto = reader["assunto"].ToString(),
                        Cliente = reader["nomeCliente"].ToString(),
                        DataAbertura = Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy"),
                        HoraAbertura = TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm")
                    };

                    flowLayoutPanel1.Controls.Add(controle);
                }
            }
        }

        private void pnLista_Paint(object sender, PaintEventArgs e)
        {
            CarregarTodosChamados();
        }
    }
}
