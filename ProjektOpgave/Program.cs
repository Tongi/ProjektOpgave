using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;

namespace TemperaturValidering
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Rum> rumListe = new List<Rum>
            {
                new Rum { Navn = "Stue", Temperatur = 22.5, Type = "Ophold", Person = "Ingen", LysTændt = true, Energiforbrug = 100, FastTemperatur = null },
                new Rum { Navn = "Køkken", Temperatur = 24.0, Type = "Madlavning", Person = "Ingen", LysTændt = false, Energiforbrug = 50, FastTemperatur = null },
                new Rum { Navn = "Soveværelse", Temperatur = 19.0, Type = "Søvn", Person = "Ingen", LysTændt = false, Energiforbrug = 30, FastTemperatur = null },
                new Rum { Navn = "Badeværelse", Temperatur = 26.5, Type = "Personlig pleje", Person = "Ingen", LysTændt = true, Energiforbrug = 75, FastTemperatur = null }
            };

            List<string> personer = new List<string> { "Far", "Mor", "Barn1", "Barn2", "Barn3" };
            Random random = new Random();

            // Start baggrundsopgaver
            Task.Run(() => TemperaturHandler.OpdaterTemperaturerAsync(rumListe, random));
            Task.Run(() => PersonHandler.FlytPersonerAsync(rumListe, personer, random));
            Task.Run(() => StartWebServer(rumListe));

            await OpdaterVisningOgMenuAsync(rumListe);
        }
        static void CheckVpnConnection()
        {
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2";  // 2 = Connected
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject networkAdapter in searcher.Get())
            {
                string name = networkAdapter["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && name.Contains("VPN"))
                {
                    Console.WriteLine($"VPN-tilkobling {name} er aktiv.");
                    return; 
                }
            }

            Console.WriteLine("Ingen VPN-tilkobling aktiv.");
        }
        static async Task OpdaterVisningOgMenuAsync(List<Rum> rumListe)
        {
           
            while (true)
            {
                Console.Clear();
              
                CheckVpnConnection();


                Console.WriteLine("\nMulige handlinger:\n1. Tænd eller sluk lys i et rum\n2. Sæt fast temperatur for et rum\n3. Afslut");

                if (Console.KeyAvailable)
                {


                    string valg = Console.ReadLine();

                    if (valg == "1")
                    {
                        Console.WriteLine("Vælg et rum:");
                        for (int i = 0; i < rumListe.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {rumListe[i].Navn}");
                        }

                        if (int.TryParse(Console.ReadLine(), out int rumIndex) && rumIndex > 0 && rumIndex <= rumListe.Count)
                        {
                            var rum = rumListe[rumIndex - 1];
                            rum.LysTændt = !rum.LysTændt;
                            rumListe[rumIndex - 1] = rum;
                            Console.WriteLine($"Lys i {rum.Navn} er nu {(rum.LysTændt ? "tændt" : "slukket")}.");
                        }
                    }
                    else if (valg == "2")
                    {
                        Console.WriteLine("Vælg et rum:");
                        for (int i = 0; i < rumListe.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {rumListe[i].Navn}");
                        }

                        if (int.TryParse(Console.ReadLine(), out int rumIndex) && rumIndex > 0 && rumIndex <= rumListe.Count)
                        {
                            Console.Write("Indtast den ønskede faste temperatur: ");
                            if (double.TryParse(Console.ReadLine(), out double fastTemperatur))
                            {
                                var rum = rumListe[rumIndex - 1];
                                rum.FastTemperatur = fastTemperatur;
                                rumListe[rumIndex - 1] = rum;
                                Console.WriteLine($"Fast temperatur for {rum.Navn} er sat til {fastTemperatur:F1} °C.");
                            }
                        }
                    }
                    else if (valg == "3")
                    {
                        break;
                    }
                }

                await Task.Delay(500);
            }
        }

        static async Task StartWebServer(List<Rum> rumListe)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Webserver kører på http://localhost:8080/");

            string html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Rumoversigt</title>
    <style>
        body { font-family: Arial, sans-serif; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 8px 12px; border: 1px solid #ddd; }
        th { background-color: #f4f4f4; }
    </style>
</head>
<body>
    <h1>Rumoversigt</h1>
    <table>
        <tbody id='rum-tbody'></tbody>
    </table>

    <script>
        async function fetchRums() {
            try {
                const response = await fetch('/rums');
                const rums = await response.json();

                const tbody = document.getElementById('rum-tbody');
                tbody.innerHTML = '';

                rums.forEach(rum => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${rum.Navn}</td>
                         <td>${rum.Type}</td>
                        <td>${rum.Temperatur.toFixed(1)} °C</td>
                        <td>${rum.Person}</td>
                        <td>${rum.LysTændt ? 'Ja' : 'Nej'}</td>
                        <td>${rum.Energiforbrug.toFixed(1)} W</td>
                        <td>${rum.FastTemperatur ? rum.FastTemperatur.toFixed(1) + ' °C' : 'Ikke sat'}</td>
                    `;
                    tbody.appendChild(row);
                });
            } catch (error) {
                console.error('Fejl ved hentning af data:', error);
            }
        }

        setInterval(fetchRums, 1000);
    </script>
</body>
</html>";

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerResponse response = context.Response;

                Console.WriteLine($"Indgående anmodning: {context.Request.RawUrl}");

                if (context.Request.RawUrl == "/rums")
                {
                    Console.WriteLine("Sender JSON-data...");
                    string json = JsonSerializer.Serialize(rumListe);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else if (context.Request.RawUrl == "/" || context.Request.RawUrl == "/index.html")
                {
                    Console.WriteLine("Sender HTML-side...");
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);

                    response.ContentType = "text/html";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    Console.WriteLine("Ugyldig anmodning...");
                    response.StatusCode = 404;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("404 - Siden blev ikke fundet");
                    response.ContentType = "text/plain";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                response.Close();
            }
        }
    }
}
