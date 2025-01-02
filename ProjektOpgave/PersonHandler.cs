using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TemperaturValidering
{
    public static class PersonHandler
    {
        public static async Task FlytPersonerAsync(List<Rum> rumListe, List<string> personer, Random random)
        {
            while (true)
            {
                foreach (string person in personer)
                {
                    int tilfældigRumIndex = random.Next(rumListe.Count);

                    for (int i = 0; i < rumListe.Count; i++)
                    {
                        if (rumListe[i].Person == person)
                        {
                            var rum = rumListe[i];
                            rum.Person = "Ingen";
                            rumListe[i] = rum;
                        }
                    }

                    var nytRum = rumListe[tilfældigRumIndex];
                    nytRum.Person = person;
                    rumListe[tilfældigRumIndex] = nytRum;
                }

                await Task.Delay(10000);
            }
        }
    }
}
