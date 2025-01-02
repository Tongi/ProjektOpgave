using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TemperaturValidering
{
    public static class TemperaturHandler
    {
        public static async Task OpdaterTemperaturerAsync(List<Rum> rumListe, Random random)
        {
            while (true)
            {
                for (int i = 0; i < rumListe.Count; i++)
                {
                    var rum = rumListe[i];

                    // Dynamisk temperaturændring
                    if (rum.FastTemperatur.HasValue)
                    {
                        if (rum.Temperatur > rum.FastTemperatur)
                        {
                            rum.Temperatur -= rum.Person != "Ingen" ? 0.3 : 0.5;
                        }
                        else if (rum.Temperatur < rum.FastTemperatur)
                        {
                            rum.Temperatur += rum.Person != "Ingen" ? 0.3 : 0.5;
                        }
                    }
                    else
                    {
                        double ændring = random.NextDouble() * 2 - 1;
                        rum.Temperatur += ændring;
                    }

                    // Dynamisk energiforbrug
                    if (rum.LysTændt)
                    {
                        // Når lys er tændt, dynamisk justering for tilstedeværelse
                        rum.Energiforbrug = 50 + random.Next(0, 21); // Basisforbrug + tilfældig variation
                        if (rum.Person != "Ingen")
                        {
                            rum.Energiforbrug += 20; // Ekstra forbrug hvis nogen er i rummet
                        }
                    }
                    else
                    {
                        // Når lys er slukket, minimal standby-forbrug
                        rum.Energiforbrug = 5 + random.Next(0, 6); // Standby-forbrug
                    }

                    rumListe[i] = rum;
                }

                await Task.Delay(500); // Hurtig opdatering
            }
        }
    }
}
