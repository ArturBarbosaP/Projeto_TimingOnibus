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

                DescarregarBrowser();

                ltx_log.Items.Clear();

                Linha linhaSelecionada = linhas[cbx_linha.SelectedIndex];
                ltx_log.Items.Add($"Nome da linha: {linhaSelecionada.NomeLinha}, {linhaSelecionada.Descricao}");

                List<PontoOnibus> pontosSelecionado = CarregarPontos(linhaSelecionada);

                PontoOnibus primeiroPonto = pontosSelecionado.First();
                ltx_log.Items.Add($"Primeiro ponto da linha selecionada: {primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                ltx_log.Items.Add($"Google Maps: https://www.google.com/maps?q={primeiroPonto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{primeiroPonto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                cbx_pontos.Items.Clear();

                foreach (var ponto in pontosSelecionado)
                {
                    cbx_pontos.Items.Add($"ID: {ponto.Id} | Coord: {ponto.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {ponto.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                }

                if (cbx_pontos.Items.Count > 0)
                    cbx_pontos.SelectedIndex = 0;

                ltx_log.Items.Add("Pegando os dados da API");
                List<Onibus> onibusLinha = await RequisicaoAPI(linhaSelecionada.CodigoInterno);

                CarregarBrowser(pontosSelecionado, onibusLinha);
            }
            catch (Exception ex)
            {
                ltx_log.Items.Add(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void CarregarBrowser(List<PontoOnibus> pontos, List<Onibus> onibus)
        {
            string html = GerarHTML(pontos, onibus);
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

        private string GerarHTML(List<PontoOnibus> pontos, List<Onibus> onibus)
        {
            string iconePontoSVG = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(@"<svg height=""256px"" width=""256px"" version=""1.1"" id=""Capa_1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" viewBox=""0 0 412.69 412.69"" xml:space=""preserve"" fill=""#4555c9"" stroke=""#4555c9"" stroke-width=""0.004126920000000001""><g id=""SVGRepo_bgCarrier"" stroke-width=""0""></g><g id=""SVGRepo_tracerCarrier"" stroke-linecap=""round"" stroke-linejoin=""round""></g><g id=""SVGRepo_iconCarrier""> <g> <g> <g> <path style=""fill:#4555c9;"" d=""M103.705,125.498h41.025v31.773h-41.025V125.498z M75.645,157.27h22.686v-31.831l-22.637-0.029 L75.645,157.27z M113.104,187.812c-4.787,0-8.627,3.84-8.627,8.588c0,4.787,3.84,8.608,8.627,8.608 c4.729,0,8.549-3.82,8.549-8.608C121.653,191.652,117.853,187.812,113.104,187.812z M276.256,187.812 c-4.778,0-8.608,3.84-8.608,8.588c0,4.787,3.83,8.608,8.608,8.608c4.719,0,8.568-3.82,8.568-8.608 C284.834,191.652,280.995,187.812,276.256,187.812z M150.114,157.27h41.044v-31.694l-41.044-0.068V157.27z M196.551,191.622 h41.035v-65.87l-41.035-0.186V191.622z M219.306,129.22h13.493v57.116h-13.493V129.22z M202.12,129.22h12.955v57.116H202.12 V129.22z M242.94,157.27h41.044v-31.538l-41.044-0.088V157.27z M289.378,125.742v31.528h51.538l-6.81-31.44L289.378,125.742z M46.364,0v319.973h100.662l59.324,92.719l59.324-92.719h100.652V0H46.364z M113.153,215.833 c-10.835,0-19.491-8.686-19.491-19.472c0-10.669,8.656-19.364,19.491-19.364c10.649,0,19.335,8.695,19.335,19.364 C132.498,207.137,123.822,215.833,113.153,215.833z M276.286,215.833c-10.786,0-19.404-8.686-19.404-19.472 c0-10.669,8.627-19.364,19.404-19.364c10.708,0,19.364,8.695,19.364,19.364C295.689,207.137,286.994,215.833,276.286,215.833z M347.832,198.119H300.74l0.088-1.739c0-13.551-10.972-24.533-24.543-24.533c-13.61,0-24.601,10.982-24.601,24.533l0.059,1.739 H137.569l0.068-1.739c0-13.551-10.962-24.533-24.484-24.533c-13.668,0-24.64,10.982-24.64,24.533l0.049,1.739h-20.39v-79.353 h269.803l9.868,43.692L347.832,198.119L347.832,198.119z""></path> </g> </g> </g> </g></svg>"));

            string iconeOnibusSVG = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(@"<svg fill=""#588cd0"" width=""256px"" height=""256px"" viewBox=""-5 -5 60.00 60.00"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" stroke=""#588cd0""><g id=""SVGRepo_bgCarrier"" stroke-width=""0"" transform=""translate(0,0), scale(1)""><rect x=""-5"" y=""-5"" width=""60.00"" height=""60.00"" rx=""16.8"" fill=""#fbfabc"" strokewidth=""0""></rect></g><g id=""SVGRepo_tracerCarrier"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke=""#CCCCCC"" stroke-width=""0.2""></g><g id=""SVGRepo_iconCarrier""><path d=""M12 0C5.4375 0 3 2.167969 3 8L3 41C3 42.359375 3.398438 43.339844 4 44.0625L4 47C4 48.652344 5.347656 50 7 50L11 50C12.652344 50 14 48.652344 14 47L14 46L36 46L36 47C36 48.652344 37.347656 50 39 50L43 50C44.652344 50 46 48.652344 46 47L46 44.0625C46.601563 43.339844 47 42.359375 47 41L47 9C47 4.644531 46.460938 0 40 0 Z M 15 4L36 4C36.554688 4 37 4.449219 37 5L37 7C37 7.550781 36.554688 8 36 8L15 8C14.449219 8 14 7.550781 14 7L14 5C14 4.449219 14.449219 4 15 4 Z M 11 11L39 11C41 11 42 12 42 14L42 26C42 28 40.046875 28.9375 39 28.9375L11 29C9 29 8 28 8 26L8 14C8 12 9 11 11 11 Z M 2 12C0.898438 12 0 12.898438 0 14L0 22C0 23.101563 0.898438 24 2 24 Z M 48 12L48 24C49.105469 24 50 23.101563 50 22L50 14C50 12.898438 49.105469 12 48 12 Z M 11.5 34C13.433594 34 15 35.566406 15 37.5C15 39.433594 13.433594 41 11.5 41C9.566406 41 8 39.433594 8 37.5C8 35.566406 9.566406 34 11.5 34 Z M 38.5 34C40.433594 34 42 35.566406 42 37.5C42 39.433594 40.433594 41 38.5 41C36.566406 41 35 39.433594 35 37.5C35 35.566406 36.566406 34 38.5 34Z""></path></g></svg>"));

            string markersPontos = "";
            string markersOnibus = "";

            foreach (var p in pontos)
            {
                markersPontos += $@"L.marker([{p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{icon: iconePonto}}).addTo(map).bindPopup('<b>Ponto de Ônibus</b><br>ID: {p.IdentificadorPonto}<br>Origem: {p.Origem}<br>Sub-linha: {p.NomeSubLinha}');";
            }

            if (onibus != null && onibus.Count > 0)
            {
                foreach (var o in onibus)
                {
                    markersOnibus += $@"L.marker([{o.LT}, {o.LG}], {{icon: iconeOnibus}}).addTo(map).bindPopup('<b>Ônibus {o.NV}</b><br>Velocidade: {o.VL} km/h<br>Direção: {o.DG}');";
                }
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
                        var iconePonto = L.icon({{
                            iconUrl: 'data:image/svg+xml;base64,{iconePontoSVG}',
                            iconSize: [25, 41],
                            iconAnchor: [12, 41],
                            popupAnchor: [1, -34]
                        }});
                        var iconeOnibus = L.icon({{
                            iconUrl: 'data:image/svg+xml;base64,{iconeOnibusSVG}',
                            iconSize: [25, 41],
                            iconAnchor: [12, 41],
                            popupAnchor: [1, -34]
                        }});
                        {markersPontos}
                        {markersOnibus}
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

        private async Task<List<Onibus>> RequisicaoAPI(string codLinha)
        {
            string url = "https://temporeal.pbh.gov.br/?param=D";

            try
            {
                var jsonResponse = await MakeGET(url);

                ltx_log.Items.Add("Resposta recebida! ");

                var onibusLinha = jsonResponse
                    .Where(p => p.NL == codLinha && p.SV != "0")
                    .GroupBy(p => p.NV)
                    .Select(p => p.First())
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