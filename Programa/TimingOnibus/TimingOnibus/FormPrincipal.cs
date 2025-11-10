using System.Text.Json;
using static System.Windows.Forms.LinkLabel;

namespace TimingOnibus
{
    public partial class FormPrincipal : Form
    {
        public FormPrincipal()
        {
            InitializeComponent();
        }

        List<Linha> linhas = new();

        private async void FormPrincipal_Load(object sender, EventArgs e)
        {
            try
            {
                await webView21.EnsureCoreWebView2Async(null);

                linhas = CarregarLinhas();

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

                ltx_log.Items.Clear();

                Linha linhaSelecionada = linhas[cbx_linha.SelectedIndex];
                ltx_log.Items.Add($"Nome da linha: {linhaSelecionada.NomeLinha}, {linhaSelecionada.Descricao}");

                List<PontoOnibus> pontosSelecionado = CarregarPontos(linhaSelecionada);

                PontoOnibus primeiroPonto = pontosSelecionado.First();
                ltx_log.Items.Add($"Primeiro ponto da linha selecionada: {primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                ltx_log.Items.Add($"Google Maps: https://www.google.com/maps?q={primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                cbx_pontos.Items.Clear();

                CarregarBrowser(pontosSelecionado);

                foreach (var ponto in pontosSelecionado)
                {
                    cbx_pontos.Items.Add($"ID: {ponto.Id} | Coord: {ponto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {ponto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                }

                if (cbx_pontos.Items.Count > 0)
                    cbx_pontos.SelectedIndex = 0;

                ltx_log.Items.Add("Pegando os dados da API");
                await RequisicaoAPI(linhaSelecionada.CodigoInterno);
            }
            catch (Exception ex)
            {
                ltx_log.Items.Add(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void CarregarBrowser(List<PontoOnibus> pontos)
        {
            string html = GerarHTML(pontos);
            webView21.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView21.CoreWebView2.Settings.AreHostObjectsAllowed = true;

            webView21.NavigateToString(html);
        }

        private string GerarHTML(List<PontoOnibus> pontos)
        {
            string markers = "";

            foreach (var p in pontos)
            {
                markers += $"L.marker([{p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}]).addTo(map);";
            }

            string lat = pontos[0].Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lng = pontos[0].Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                      <meta charset='utf-8'>
                      <title>Mapa de Ônibus (OpenStreetMap)</title>
                      <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
                      <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
                      <style>html,body,#map{{height:100%;margin:0;padding:0}}</style>
                    </head>
                    <body>
                      <div id='map'></div>
                      <script>
                        var map = L.map('map').setView([{lat}, {lng}], 13);
                        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                            attribution: '© OpenStreetMap contributors'
                        }}).addTo(map);
                        {markers}
                      </script>
                    </body>
                    </html>";
        }

        private List<Linha> CarregarLinhas()
        {
            try
            {
                string caminhoArquivo = "D:\\Arquivos\\Documents\\Escola\\Projeto_TimingOnibus\\Dados\\ConversaoCodigoLinha.json";

                string jsonContent = System.IO.File.ReadAllText(caminhoArquivo);

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };

                var dadosLinhas = JsonSerializer.Deserialize<DadosLinhas>(jsonContent, options);

                List<Linha> linhas = [];

                foreach (var record in dadosLinhas.records)
                {
                    linhas.Add(new Linha
                    {
                        CodigoInterno = record[1].ToString(),
                        NomeLinha = record[2].ToString(),
                        Descricao = record[3].ToString()
                    });
                }

                return linhas ?? throw new Exception("A linha digitada não existe!");
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na função CarregarLinhas: " + ex.Message);
            }
        }

        private List<PontoOnibus> CarregarPontos(Linha linhaSelecionada)
        {
            try
            {
                string caminhoArquivo = "D:\\Arquivos\\Documents\\Escola\\Projeto_TimingOnibus\\Dados\\20251001_ponto_onibus.csv";

                string csvContent = System.IO.File.ReadAllText(caminhoArquivo);

                List<PontoOnibus> pontos = [];
                var linhas = csvContent.Split('\n');

                for (int i = 0; i < linhas.Length; i++)
                {
                    var linha = linhas[i].Trim();
                    if (string.IsNullOrEmpty(linha)) continue;

                    var parts = linha.Split(';');
                    if (parts.Length >= 7)
                    {
                        try
                        {
                            var geometria = parts[6].Trim();
                            var coordenadas = geometria.Replace("POINT (", "").Replace(")", "").Split(' ');

                            if (coordenadas.Length == 2)
                            {
                                double utmX = double.Parse(coordenadas[0], System.Globalization.CultureInfo.InvariantCulture);
                                double utmY = double.Parse(coordenadas[1], System.Globalization.CultureInfo.InvariantCulture);

                                var (lat, lon) = Utils.ConverterUtmParaLatLong(utmX, utmY);

                                pontos.Add(new PontoOnibus
                                {
                                    Id = parts[0].Trim(),
                                    CodigoLinha = parts[1].Trim(),
                                    NomeLinha = parts[2].Trim(),
                                    NomeSubLinha = parts[3].Trim(),
                                    Origem = parts[4].Trim(),
                                    IdentificadorPonto = parts[5].Trim(),
                                    Latitude = lat,
                                    Longitude = lon
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            ltx_log.Items.Add($"ERRO linha {i}: {ex.Message}");
                            Console.WriteLine($"ERRO linha {i}: {ex.Message}");
                            continue;
                        }
                    }
                }

                List<PontoOnibus> pontosLinha = pontos
                    .Where(p => p.CodigoLinha == linhaSelecionada.NomeLinha.Split('-').First())
                    .Where(p => Utils.CleanString(linhaSelecionada.Descricao) != Utils.CleanString(p.NomeLinha) ? 
                    linhaSelecionada.Descricao == p.NomeSubLinha : 
                    p.NomeSubLinha == "PRINCIPAL").ToList();

                return pontosLinha ?? throw new Exception("Nenhum ponto encontrado para a linha selecionada!");
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na função CarregarPontos: " + ex.Message);
            }
        }

        private async Task RequisicaoAPI(string codLinha)
        {
            string url = "https://temporeal.pbh.gov.br/?param=D";

            try
            {
                var jsonResponse = await MakeGET(url);

                ltx_log.Items.Add("Resposta recebida! ");

                var onibusLinha = jsonResponse
                    .Where(p => p.NL == codLinha && p.SV != "0")
                    .GroupBy(p => p.NV)
                    .Select(p => p.First());

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
                client.DefaultRequestHeaders.Add("USer-Agent", "BusTiming/0.1");

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
    }
}