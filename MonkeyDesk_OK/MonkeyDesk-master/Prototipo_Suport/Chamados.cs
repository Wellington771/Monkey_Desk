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
        public int UsuarioLogadoId { get; set; }

        string strConexao = @"Data Source=.\SQLEXPRESS;Initial Catalog=SuporteChamados;Integrated Security=True";
        SqlConnection objconexao;

        private int _idChamado;
        public int IdChamado
        {
            get { return _idChamado; }
            set
            {
                _idChamado = value;
                if (_idChamado != 0)
                    CarregarChamado();
            }
        }

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
            set => lbAssunto.Text = "" + value;
        }

        public string Cliente
        {
            get => lbCliente.Text;
            set => lbCliente.Text = "" + value;
        }

        public string DataAbertura
        {
            get => mtbData.Text;
            set => mtbData.Text = "" + value;
        }

        public string HoraAbertura
        {
            get => mtbHora.Text;
            set => mtbHora.Text = "" + value;
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

        private void CarregarChamado()
        {
            if (_idChamado == 0)
                return;

            using (SqlConnection conn = new SqlConnection(strConexao))
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

                cmd.Parameters.AddWithValue("@idChamado", _idChamado);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    lbIdChamado.Text = "" + reader["idChamado"].ToString();
                    lbCliente.Text = "Cliente: " + reader["nomeCliente"].ToString();
                    Assunto = reader["assunto"].ToString();
                    DataAbertura = Convert.ToDateTime(reader["dataAbertura"]).ToString("dd/MM/yyyy");
                    HoraAbertura = TimeSpan.Parse(reader["horaAbertura"].ToString()).ToString(@"hh\:mm");
                }
            }
        }

        private void CarregarStatus()
        {
            cmbStatus.Items.AddRange(new string[] { "Aberto", "Em andamento", "Pendente", "Resolvido", "Fechado" });
        }

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
            if (_idChamado == 0) return;

            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE tblChamados
                    SET statuss = @status, tecnicoResponsavel = @tecnico
                    WHERE idChamado = @id", conn);

                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@tecnico", cmbTecnico.SelectedValue);
                cmd.Parameters.AddWithValue("@id", _idChamado);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            AdicionarHistorico(comentarioAlteracao);
        }

        private void AdicionarHistorico(string comentarioAlteracao)
        {
            using (SqlConnection conn = new SqlConnection(strConexao))
            {
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO tblHistoricoChamado (idChamado, descricaoAlteracao, idUsuario)
                    VALUES (@idChamado, @descricaoAlteracao, @idUsuario)", conn);

                cmd.Parameters.AddWithValue("@idChamado", _idChamado);
                cmd.Parameters.AddWithValue("@descricaoAlteracao", comentarioAlteracao);
                cmd.Parameters.AddWithValue("@idUsuario", UsuarioLogadoId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void CarregarHistoricoChamado(int idChamado)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(strConexao))
                {
                    string sql = @"
                SELECT 
                    h.dataAlteracao AS Data,
                    u.usuario AS Tecnico,
                    h.descricaoAlteracao AS Descricao
                FROM tblHistoricoChamado h
                INNER JOIN tblUsuarios u ON h.idUsuario = u.idUsuario
                WHERE h.idChamado = @idChamado
                ORDER BY h.dataAlteracao DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@idChamado", idChamado);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    MessageBox.Show("Linhas carregadas: " + dt.Rows.Count);

                    dgvHistorico.DataSource = null;
                    dgvHistorico.DataSource = dt;

                    dgvHistorico.AutoGenerateColumns = true;

                    if (dgvHistorico.Columns["Data"] != null)
                        dgvHistorico.Columns["Data"].Width = 130;
                    if (dgvHistorico.Columns["Tecnico"] != null)
                        dgvHistorico.Columns["Tecnico"].Width = 150;
                    if (dgvHistorico.Columns["Descricao"] != null)
                        dgvHistorico.Columns["Descricao"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar histórico: " + ex.Message);
            }
        }



        private void btnHistorico_Click(object sender, EventArgs e)
        {
            if (_idChamado == 0)
            {
                MessageBox.Show("Não há um chamado válido para visualizar o histórico.");
                return;
            }

            dgvHistorico.Visible = !dgvHistorico.Visible;

            if (dgvHistorico.Visible)
            {
                CarregarHistoricoChamado(_idChamado);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (_idChamado == 0)
            {
                MessageBox.Show("Não há um chamado válido para salvar.");
                return;
            }

            string comentario = txtDescricao.Text.Trim();

            if (string.IsNullOrEmpty(comentario))
            {
                MessageBox.Show("Por favor, insira um comentário antes de salvar.");
                return;
            }

            AtualizarChamado(comentario);

            MessageBox.Show("Chamado atualizado com sucesso!");

            txtDescricao.Clear();
        }

        private void cmbTecnico_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_idChamado != 0)
                AtualizarChamado("Alteração de técnico para " + cmbTecnico.Text);
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_idChamado != 0)
                AtualizarChamado("Alteração de status para " + cmbStatus.Text);
        }
    }
}
