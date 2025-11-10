using System.Text.Json;

namespace TimingOnibus
{
    public partial class FormPrincipal : Form
    {
        public FormPrincipal()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private List<Linha> linhas = new();
        private List<PontoOnibus> pontosSelecionado = new();
        private List<Onibus> onibusLinha = new();
        private PontoOnibus? pontoMapa = null;

        private async void InitializeWebView()
        {
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.AddHostObjectToScript("external", new ScriptInterface(this));
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            try
            {
                linhas = Utils.CarregarLinhas();

                cbx_linha.AutoCompleteMode = AutoCompleteMode.Suggest;
                cbx_linha.AutoCompleteSource = AutoCompleteSource.ListItems;

                foreach (var linha in linhas)
                {
                    cbx_linha.Items.Add($"{linha.NomeLinha} - {linha.Descricao}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro no Load do form: " + ex.Message);
            }
        }

        private async void btn_calcular_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbx_linha.SelectedIndex == -1)
                    return;

                DescarregarBrowser();

                ltx_log.Items.Clear();
                cbx_origem.Items.Clear();

                Linha linhaSelecionada = linhas[cbx_linha.SelectedIndex];
                ltx_log.Items.Add($"Nome da linha: {linhaSelecionada.NomeLinha}, {linhaSelecionada.Descricao}");

                pontosSelecionado = Utils.CarregarPontos(linhaSelecionada);

                PontoOnibus primeiroPonto = pontosSelecionado.First();
                ltx_log.Items.Add($"Primeiro ponto da linha selecionada: {primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                ltx_log.Items.Add($"Google Maps: https://www.google.com/maps?q={primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                ltx_log.Items.Add("Pegando os dados da API");
                onibusLinha = await RequisicaoAPI(linhaSelecionada.CodigoInterno);

                cbx_origem.Items.Add("TODAS");

                var sentidos = onibusLinha.DistinctBy(o => o.SV).OrderBy(o => o.SV);

                if (sentidos.Any())
                {
                    foreach (var item in sentidos)
                    {
                        cbx_origem.Items.Add($"Sentido: {item.SV} | {linhas.FirstOrDefault(l => l.CodigoInterno == item.NL).Descricao.Split('/')[int.Parse(item.SV) - 1]}");
                    }
                }

                cbx_origem.SelectedIndex = 0;

                CarregarBrowser(pontosSelecionado, onibusLinha);
            }
            catch (Exception ex)
            {
                ltx_log.Items.Add(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void cbx_origem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbx_origem.SelectedIndex != 0 && cbx_origem.SelectedItem != "TODAS")
            {
                var tempPontos = pontosSelecionado;
                pontosSelecionado = tempPontos.Where(p => Utils.CleanString(p.Origem) == Utils.CleanString(cbx_origem.SelectedItem.ToString().Split('|').Last())).ToList();

                var tempOnibus = onibusLinha;
                onibusLinha = tempOnibus.Where(o => o.SV == cbx_origem.SelectedItem.ToString().Split('|').First().Replace("Sentido: ", "").Trim()).ToList();

                CarregarBrowser(pontosSelecionado, onibusLinha);
            }
        }

        private void CarregarBrowser(List<PontoOnibus> pontos, List<Onibus> onibus)
        {
            string html = Utils.GerarHTML(pontos, onibus);
            webView21.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView21.CoreWebView2.Settings.AreHostObjectsAllowed = true;

            webView21.NavigateToString(html);
        }

        private void DescarregarBrowser()
        {
            webView21.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView21.CoreWebView2.Settings.AreHostObjectsAllowed = true;

            webView21.NavigateToString(Utils.EmptyHTML());
        }

        public void OnPontoSelecionado(string idPonto)
        {
            if (onibusLinha.DistinctBy(o => o.SV).OrderBy(o => o.SV).Any() && cbx_origem.SelectedIndex == 0)
            {
                MessageBox.Show("Filtre pela origem para calcular a distancia do ônibus ao ponto");
                return;
            }

            pontoMapa = pontosSelecionado.FirstOrDefault(p => p.IdentificadorPonto == idPonto);

            if (pontoMapa is not null)
            {
                MessageBox.Show($"Ponto selecionado!\n" +
                      $"ID: {pontoMapa.IdentificadorPonto}\n" +
                      $"Origem: {pontoMapa.Origem}\n" +
                      $"Lat: {pontoMapa.Latitude}\n" +
                      $"Lon: {pontoMapa.Longitude}\n\n" +
                      $"Agora você pode calcular o tempo dos ônibus até este ponto!",
                      "Ponto Selecionado",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);

                CalcularTempoPrevisao(pontoMapa);
            }
        }

        private void CalcularTempoPrevisao(PontoOnibus ponto)
        {
            foreach (var onibus in onibusLinha)
            {
                if (!double.TryParse(onibus.LT, System.Globalization.CultureInfo.InvariantCulture, out double latOnibus) ||
                    !double.TryParse(onibus.LG, System.Globalization.CultureInfo.InvariantCulture, out double lonOnibus))
                    throw new Exception("Erro na função CalcularTempoPrevisao ao passar para double as coordenadas do ônibus!");

                double distanciaReta = Utils.CalcularDistanciaHaversine(latOnibus, lonOnibus, ponto.Latitude, ponto.Longitude);

                double distanciaReal = distanciaReta * 1.4;

                if (!double.TryParse(onibus.VL, out double velocidade))
                    throw new Exception("Erro na função CalcularTempoPrevisao ao passar para double a velocidade do ônibus!");

                //ajustar velocidade de acordo com o horário de pico

                double tempoHoras = distanciaReal / velocidade;
                double tempoMinutos = tempoHoras * 60;

                onibus.TempoDouble = Math.Round(tempoMinutos, 0);
                onibus.PrevisaoTempo = $"{Math.Round(tempoMinutos, 0)} minutos";
            }

            ltx_log.Items.Add("Tempo calculado para todos os ônibus!");

            var onibusPerto = onibusLinha.OrderBy(o => o.TempoDouble).First();

            ltx_log.Items.Add($"Ônibus mais próximo: {onibusPerto.NV} - Tempo: {onibusPerto.PrevisaoTempo}");

            CarregarBrowser(pontosSelecionado, onibusLinha);
        }

        private async Task<List<Onibus>> RequisicaoAPI(string codLinha)
        {
            string url = "https://temporeal.pbh.gov.br/?param=D";

            try
            {
                var jsonResponse = await MakeGET(url);

                ltx_log.Items.Add("Resposta recebida! ");

                var onibusLinha = jsonResponse
                    .Where(p => p.NL == codLinha && p.SV != null && p.SV != "" && p.SV != "0" && p.SV != "3")
                    .DistinctBy(p => p.NV)
                    .ToList();

                if (!onibusLinha.Any())
                    throw new Exception("Nenhum ônibus encontrado!");

                ltx_log.Items.Add($"Foi encontrado {onibusLinha.Count()} ônibus da linha selecionada");
                ltx_log.Items.Add("Dados dos ônibus: ");
                ltx_log.Items.Add("");

                foreach (var bus in onibusLinha)
                {
                    ltx_log.Items.Add($"Veículo: {bus.NV}");
                    ltx_log.Items.Add($"Velocidade: {bus.VL}");
                    ltx_log.Items.Add($"Direção: {bus.DG}");
                    ltx_log.Items.Add($"Sentido: {bus.SV}");
                    ltx_log.Items.Add($"Distância percorrida: {bus.DT}");
                    ltx_log.Items.Add($"Data e hora: {bus.HR}");
                    ltx_log.Items.Add($"Latitude: {bus.LT}");
                    ltx_log.Items.Add($"Longitude: {bus.LG}");
                    ltx_log.Items.Add($"Evento: {bus.EV}");
                    ltx_log.Items.Add("");
                }

                return onibusLinha;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro na função RequisicaoAPI: " + ex.Message);
            }
        }

        private async Task<List<Onibus>> MakeGET(string url)
        {
            try
            {
                HttpClient client = new();

                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("USer-Agent", "TimingOnibus/0.1");

                ltx_log.Items.Add("Iniciando a requisição ao servidor");

                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Onibus> dados = JsonSerializer.Deserialize<List<Onibus>>(jsonResponse, options);

                return dados;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na função MakeGET: " + ex.Message);
            }
        }
    }

    public class Linha
    {
        public string CodigoInterno { get; set; } //codigo da linha que vem na resposta da API
        public string NomeLinha { get; set; } //linha que é exibida nos onibus
        public string Descricao { get; set; } //descricao da linha
    }

    public class DadosLinhas
    {
        public List<Field> fields { get; set; }
        public List<List<JsonElement>> records { get; set; }
    }

    public class Field
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class PontoOnibus
    {
        public string Id { get; set; }
        public string CodigoLinha { get; set; }
        public string NomeLinha { get; set; }
        public string NomeSubLinha { get; set; }
        public string Origem { get; set; }
        public string IdentificadorPonto { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Onibus
    {
        public string EV { get; set; } //Código do evento. O evento 105 é um evento de posição
        public string HR { get; set; } //Data/hora: ano (4 caracteres), mês (2 caracteres), dia (2 caracteres), hora (2 caracteres), minuto (2 caracteres), segundo (2 caracteres)
        public string LT { get; set; } //Latitude em WGS84 fuso 23S
        public string LG { get; set; } //Longitude em WGS84 fuso 23S
        public string NV { get; set; } //Número de ordem do veículo
        public string VL { get; set; } //Velocidade instantânea do veículo
        public string NL { get; set; } //Código do número de linha (arquivo de conversão das linhas do sistema convencional)
        public string DG { get; set; } //Direção do veículo
        public string SV { get; set; } //Sentido do veículo em uma viagem ((1) ida, (2) volta)
        public string DT { get; set; } //Distância percorrida
        public string PrevisaoTempo = "Sem previsao"; //estimativa do tempo de chegada do ônibus no ponto selecionado em texto
        public double TempoDouble { get; set; } //estimativa do tempo de chegada do ônibus no ponto selecionado
    }
}