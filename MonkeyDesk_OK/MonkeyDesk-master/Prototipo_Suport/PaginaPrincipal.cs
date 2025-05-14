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
            DialogResult resultado = MessageBox.Show("Deseja realmente criar um novo chamado?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                int idChamadoGerado = -1;
                DateTime dataAbertura = DateTime.Now;

                using (SqlConnection conexao = new SqlConnection(strconexao))
                {
                    try
                    {
                        conexao.Open();

                        // Inserir cliente temporário
                        string sqlCliente = "INSERT INTO tblClientes (nomeCliente) VALUES ('Temporário')";
                        SqlCommand cmdCliente = new SqlCommand(sqlCliente, conexao);
                        cmdCliente.ExecuteNonQuery();

                        // Obter ID do cliente
                        SqlCommand cmdGetIdCliente = new SqlCommand("SELECT SCOPE_IDENTITY()", conexao);
                        int idCliente = Convert.ToInt32(cmdGetIdCliente.ExecuteScalar());

                        // Inserir chamado com ID gerado
                        string sqlChamado = @"
                            INSERT INTO tblChamados 
                            (idCliente, idUsuario, assunto, descricao, dataAbertura, horaAbertura, prioridade, statuss, tecnicoResponsavel)
                            VALUES
                            (@idCliente, 1, '', '', @dataAbertura, CONVERT(TIME, GETDATE()), 'Baixo', 'Aberto', 2);
                            SELECT SCOPE_IDENTITY();";

                        SqlCommand cmdChamado = new SqlCommand(sqlChamado, conexao);
                        cmdChamado.Parameters.AddWithValue("@idCliente", idCliente);
                        cmdChamado.Parameters.AddWithValue("@dataAbertura", dataAbertura);

                        idChamadoGerado = Convert.ToInt32(cmdChamado.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao gerar ID do chamado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Abrir a próxima tela com o ID gerado
                TelaDeChamadocs telaDeChamadocs = new TelaDeChamadocs(idChamadoGerado);
                telaDeChamadocs.Show();
                this.Hide();
            }
        }
    }
}
