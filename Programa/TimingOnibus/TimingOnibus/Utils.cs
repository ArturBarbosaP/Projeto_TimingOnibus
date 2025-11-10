using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimingOnibus
{
    static class Utils
    {
        public static string CleanString(string str)
        {
            string normalized = str.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new();

            foreach (var ch in normalized)
            {
                UnicodeCategory unicodeCat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCat != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            string semAcento = sb.ToString().Normalize(NormalizationForm.FormC);

            return new string(semAcento.Where(Char.IsLetterOrDigit).ToArray());
        }

        public static (double latitude, double longitude) ConverterUtmParaLatLong(double utmX, double utmY)
        {
            // Conversão de EPSG:31983 (SIRGAS 2000 / UTM zone 23S) para WGS84
            // Parâmetros da zona UTM 23S
            const double a = 6378137.0; // Semi-eixo maior (metros)
            const double f = 1 / 298.257223563; // Achatamento
            const double k0 = 0.9996; // Fator de escala
            const int zone = 23; // Zona UTM
            const double falseEasting = 500000.0;
            const double falseNorthing = 10000000.0; // Hemisfério Sul

            double e = Math.Sqrt(2 * f - f * f); // Excentricidade
            double e2 = e * e;
            double e4 = e2 * e2;
            double e6 = e4 * e2;

            // Remover false easting e northing
            double x = utmX - falseEasting;
            double y = utmY - falseNorthing;

            // Meridiano central da zona
            double lon0 = (zone * 6 - 183) * Math.PI / 180.0;

            // Latitude footpoint
            double M = y / k0;
            double mu = M / (a * (1 - e2 / 4 - 3 * e4 / 64 - 5 * e6 / 256));

            double e1 = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2));
            double phi1 = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                        + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                        + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);

            double C1 = e2 * Math.Cos(phi1) * Math.Cos(phi1) / (1 - e2);
            double T1 = Math.Tan(phi1) * Math.Tan(phi1);
            double N1 = a / Math.Sqrt(1 - e2 * Math.Sin(phi1) * Math.Sin(phi1));
            double R1 = a * (1 - e2) / Math.Pow(1 - e2 * Math.Sin(phi1) * Math.Sin(phi1), 1.5);
            double D = x / (N1 * k0);

            // Latitude
            double lat = phi1 - (N1 * Math.Tan(phi1) / R1) *
                        (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * e2) * D * D * D * D / 24
                        + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * e2 - 3 * C1 * C1) * D * D * D * D * D * D / 720);

            // Longitude
            double lon = lon0 + (D - (1 + 2 * T1 + C1) * D * D * D / 6
                        + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * e2 + 24 * T1 * T1) * D * D * D * D * D / 120) / Math.Cos(phi1);

            // Converter para graus
            lat = lat * 180.0 / Math.PI;
            lon = lon * 180.0 / Math.PI;

            // Ajuste para hemisfério sul (coordenadas negativas)
            lat = -Math.Abs(lat);

            return (lat, lon);
        }

        public static string EmptyHTML()
        {
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
                        var map = L.map('map');
                        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                            attribution: '© OpenStreetMap contributors'
                        }}).addTo(map);
                      </script>
                    </body>
                    </html>";
        }
    }
}