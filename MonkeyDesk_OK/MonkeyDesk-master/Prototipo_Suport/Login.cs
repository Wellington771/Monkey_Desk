using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prototipo_Suport
{
    public partial class Login : Form
    {
        string strConexao = @"Data Source=.\SQLEXPRESS;Initial Catalog=SuporteChamados;Integrated Security=True";
        public int usuario;
        public Login()
        {
            InitializeComponent();
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {

            string loginInformado = txtUsuario.Text.Trim();
            string senhaInformada = txtSenha.Text.Trim();

            bool loginExiste = false;
            bool senhaCorreta = false;

            using (SqlConnection objConexao = new SqlConnection(strConexao))
            {
                try
                {
                    objConexao.Open();

                    string sql = "SELECT idUsuario, senha FROM tblUsuarios WHERE usuario = @usuario";
                    SqlCommand cmd = new SqlCommand(sql, objConexao);
                    cmd.Parameters.AddWithValue("@usuario", loginInformado);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        loginExiste = true;
                        string senhaCorretaBD = reader["senha"].ToString();

                        if (senhaInformada == senhaCorretaBD)
                        {
                            senhaCorreta = true;
                            usuario = Convert.ToInt32(reader["idUsuario"]);
                        }
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao conectar: " + ex.Message);
                    return;
                }
            }

            if (loginExiste && senhaCorreta)
            {
                telaPrincipal principal = new telaPrincipal();
                principal.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Login ou senha incorretos.", "Erro de autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

