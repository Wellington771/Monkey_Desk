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
    public partial class Chamados : UserControl
    {


        string strConexao = @"Data Source=.\SQLEXPRESS;Initial Catalog=SuporteChamados;Integrated Security=True";
        SqlConnection objconexao;

        public int IdChamado { get; set; }
        public Chamados()
        {
            InitializeComponent();
            CarregarStatus();
            CarregarTecnicos();
            dgvHistorico.Visible = false;
        }

        public string Assunto
        {
            get => lbAssunto.Text;
            set => lbAssunto.Text = "Assunto: " + value;
        }

        public string Cliente
        {
            get => lbCliente.Text;
            set => lbCliente.Text = "Cliente: " + value;
        }



        public string StatusSelecionado
        {
            get => cmbStatus.Text;
            set => cmbStatus.Text = value;
        }

        public int TecnicoSelecionado
        {
            get => Convert.ToInt32(cmbTecnico.SelectedValue);
            set => cmbTecnico.SelectedValue = value;
        }

      
        private void CarregarHistoricoChamado(int idChamado)
        {
            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand("SELECT h.dataAlteracao, u.usuario, h.descricaoAlteracao " +
                                               "FROM tblHistoricoChamado h " +
                                               "JOIN tblUsuarios u ON h.idUsuario = u.idUsuario " +
                                               "WHERE h.idChamado = @idChamado " +
                                               "ORDER BY h.dataAlteracao DESC", conn);

                cmd.Parameters.AddWithValue("@idChamado", idChamado);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Preencher o DataGridView com o histórico
                dgvHistorico.DataSource = dt;
            }
        }

        private void CarregarStatus()
        {
            cmbStatus.Items.AddRange(new string[] { "Aberto", "Em andamento", "Pendente", "Resolvido", "Fechado" });
        }

        // Carregar técnicos do banco de dados
        private void CarregarTecnicos()
        {
            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand("SELECT idUsuario, usuario FROM tblUsuarios", conn);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbTecnico.DataSource = dt;
                cmbTecnico.DisplayMember = "usuario";
                cmbTecnico.ValueMember = "idUsuario";
            }
        }

        private void AtualizarChamado(string comentarioAlteracao)
        {
            if (IdChamado == 0) return;

            // Atualizar o chamado no banco com o novo status e técnico
            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand(@"
                UPDATE tblChamados
                SET statuss = @status, tecnicoResponsavel = @tecnico
                WHERE idChamado = @id", conn);

                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@tecnico", cmbTecnico.SelectedValue);
                cmd.Parameters.AddWithValue("@id", IdChamado);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Adicionar a alteração ao histórico do chamado
            AdicionarHistorico(IdChamado, comentarioAlteracao);
        }

        private void AdicionarHistorico(int idChamado, string comentarioAlteracao)
        {
            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tblHistoricoChamado (idChamado, descricaoAlteracao, idUsuario)
                VALUES (@idChamado, @descricaoAlteracao, @idUsuario)", conn);

                // Aqui, você pode pegar o ID do usuário logado. Exemplo:
                // cmd.Parameters.AddWithValue("@idUsuario", usuarioLogadoId);  
                // Para o exemplo, vamos assumir que o ID do técnico que está fazendo a alteração é o mesmo do ComboBox:
                cmd.Parameters.AddWithValue("@idChamado", idChamado);
                cmd.Parameters.AddWithValue("@descricaoAlteracao", comentarioAlteracao);
                cmd.Parameters.AddWithValue("@idUsuario", cmbTecnico.SelectedValue);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void CarregarChamado()
        {
            if (IdChamado == 0) return;

            SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=SuporteChamados;Integrated Security=True");
            SqlCommand cmd = new SqlCommand(@"
        SELECT 
            c.idChamado,
            cli.nomeCliente,
            c.dataAbertura,
            c.horaAbertura
        FROM tblChamados c
        INNER JOIN tblClientes cli ON cli.idCliente = c.idCliente
        WHERE c.idChamado = @idChamado", conn);

            cmd.Parameters.AddWithValue("@idChamado", IdChamado);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                lbIdChamado.Text = "ID: " + reader["idChamado"].ToString();
                lbCliente.Text = "Cliente: " + reader["nomeCliente"].ToString();
                mtbData.Text = "Data: " + Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy");
                mtbHora.Text = "Hora: " + TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm");
            }
            conn.Close();
        }

        private void btnHistorico_Click(object sender, EventArgs e)
        {
            dgvHistorico.Visible = true;
            if (IdChamado == 0)
            {
                MessageBox.Show("Não há um chamado válido para visualizar o histórico.");
                return;
            }

            // Carregar histórico do chamado
            CarregarHistoricoChamado(IdChamado);
        }



        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (IdChamado == 0)
            {
                MessageBox.Show("Não há um chamado válido para salvar.");
                return;
            }

            // Texto que será registrado no histórico de alterações
            string comentario = txtDescricao.Text.Trim(); // Supondo que txtComentario seja um TextBox para o comentário do técnico

            // Verifica se o comentário não está vazio
            if (string.IsNullOrEmpty(comentario))
            {
                MessageBox.Show("Por favor, insira um comentário antes de salvar.");
                return;
            }

            // Atualizar chamado (status e técnico responsável)
            AtualizarChamado(comentario);

            // Mostrar mensagem de sucesso
            MessageBox.Show("Chamado atualizado com sucesso!");

            // Limpar campo de comentário após salvar (opcional)
            txtDescricao.Clear();
        }

        private void cmbTecnico_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarChamado("Alteração de técnico para " + cmbTecnico.Text);
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarChamado("Alteração de status para " + cmbStatus.Text);
        }
    }
}
