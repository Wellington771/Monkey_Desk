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
        SqlConnection objconexao;

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
            carregarGrafico();
            atualizarLabels();
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


        private void carregarGrafico()
        {

            try
            {
                using (objconexao = new SqlConnection(strconexao))
                {
                    string chamado = @"
                    SELECT S.nomeStatus AS Status, COUNT(C.idChamado) AS Total
                    FROM tblChamados C
                    JOIN tblStatus S ON C.idStatus = S.idStatus
                    GROUP BY S.nomeStatus";

                    SqlCommand comando = new SqlCommand(chamado, objconexao);
                    objconexao.Open();
                    SqlDataReader reader = comando.ExecuteReader();

                    charGrafico.Series.Clear(); // Limpa dados antigos

                    var serie = new System.Windows.Forms.DataVisualization.Charting.Series("Chamados por Status")
                    {
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
                    };

                    while (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        int total = Convert.ToInt32(reader["Total"]);

                        var ponto = serie.Points.AddXY(status, total);

                        // Define a cor de cada barra com base no nome do status
                        switch (status.ToLower())
                        {
                            case "aberto":
                                serie.Points[serie.Points.Count - 1].Color = Color.Lime;
                                break;
                            case "em andamento":
                                serie.Points[serie.Points.Count - 1].Color = Color.FromArgb(255, 128, 128);
                                break;
                            case "pendente":
                                serie.Points[serie.Points.Count - 1].Color = Color.Red;
                                break;
                            case "resolvido":
                                serie.Points[serie.Points.Count - 1].Color = Color.Cyan;
                                break;
                            case "fechado":
                                serie.Points[serie.Points.Count - 1].Color = Color.FromArgb(64, 64, 64);
                                break;
                            default:
                                serie.Points[serie.Points.Count - 1].Color = Color.Gray; // Cor padrão para não previstos
                                break;
                        }
                    }

                    charGrafico.Series.Add(serie);

                    // Eixos do gráfico
                    charGrafico.ChartAreas[0].AxisX.Title = "Status";
                    charGrafico.ChartAreas[0].AxisY.Title = "Qtd de Chamados";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar gráfico: " + ex.Message);
            }


        }

        private void atualizarLabels()
        {
            try
            {
                using (objconexao = new SqlConnection(strconexao))
                {
                    string query = @"
                SELECT S.nomeStatus, COUNT(C.idChamado) AS Total
                FROM tblChamados C
                JOIN tblStatus S ON C.idStatus = S.idStatus
                GROUP BY S.nomeStatus";

                    SqlCommand comando = new SqlCommand(query, objconexao);
                    objconexao.Open();
                    SqlDataReader reader = comando.ExecuteReader();

                    // Zera as labels antes de preencher
                    lbAbertos.Text = "0";
                    lbAndamento.Text = "0";
                    lbPendente.Text = "0";
                    lbResolvidos.Text = "0";
                    lbFechados.Text = "0";

                    while (reader.Read())
                    {
                        string status = reader["nomeStatus"].ToString().ToLower();
                        int total = Convert.ToInt32(reader["Total"]);

                        switch (status)
                        {
                            case "aberto":
                                lbAbertos.Text = total.ToString();
                                break;
                            case "em andamento":
                                lbAndamento.Text = total.ToString();
                                break;
                            case "pendente":
                                lbPendente.Text = total.ToString();
                                break;
                            case "resolvido":
                                lbResolvidos.Text = total.ToString();
                                break;
                            case "fechado":
                                lbFechados.Text = total.ToString();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar as labels: " + ex.Message);
            }
        }
    }
}
