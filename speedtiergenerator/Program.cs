using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace speedtiergenerator
{
    //variables for stat calculation - can be modified to be used for other tiers/purposes

    //max investment with positive nature
    static class maxSpeed
    {
        //iv = stat iv
        public static int iv = 31;
        //ev = stat evs
        public static int ev = 252;
        //level = pokemon level
        public static int level = 5;
        //nature = positive = 1.1, negative = 0.9, netural = 1
        public static double nature = 1.1;
    }

    //no investment with neutral nature
    static class neutralSpeed
    {
        //iv = stat iv
        public static int iv = 31;
        //ev = stat evs
        public static int ev = 0;
        //level = pokemon level
        public static int level = 5;
        //nature = positive = 1.1, negative = 0.9, netural = 1
        public static double nature = 1;
    }

    class Program
    {
        static void Main(string[] args)
        {
            //load pokemon in from text file
            string rawPokemon;
            string[] splitPokemon;
            try
            {
                using (StreamReader streamReader = new StreamReader("input.txt"))
                {
                    rawPokemon = streamReader.ReadToEnd();
                }
                splitPokemon = rawPokemon.Split(',');
            }
            catch (Exception)
            {
                Console.WriteLine("input.txt could not be read. Please ensure it exists.");
                throw;
            }

            //string[] doubleSpeeders = "Feebas,Goldeen,Horsea,Kabuto,Lotad,Magikarp,Surskit,Bellsprout,Exeggcute,Hoppip,Oddish,Seedot,Sunkern,Carvanha,Doduo,Dratini,Growlithe,Ledyba,Pidgey,Ponyta,Porygon,Spearow,Spinarak,Swablu,Taillow,Treecko,Wingull".Split(",");
            //createPokemonList(doubleSpeeders);

            createPokemonList(splitPokemon);
            Console.ReadKey();
        }

        static async void createPokemonList(string[] splitPokemon)
        {
            Dictionary<string, int> pokemon = new Dictionary<string, int>();
            pokemon.Clear();
            foreach (var item in splitPokemon)
            {
                int speed = await getPokemonBaseSpeed(item.ToLower());
                pokemon.Add(item, (int)calculateSpeed(speed));
            }

            generateOutput(pokemon);
            Console.WriteLine("Finished, press any key to exit");
        }

        //formats the speed tier post and writes it to output.txt
        static void generateOutput(Dictionary<string, int> pokemon)
        {
            //write out swift swimmers
            //sort the list by speed value
            pokemon = pokemon.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            using (StreamWriter sw = new StreamWriter("output.txt"))
            {
                int currentSpeed = 0;
                foreach (var item in pokemon)
                {
                    //get current speed
                    //check if its the same as last speed
                    if (currentSpeed != item.Value)
                    {
                        sw.WriteLine();
                        //sw.WriteLine("[b]" + item.Value * 2 + " speed[/b] (at +2 due to ability or Agility)");
                        sw.WriteLine("[b]" + item.Value + " speed[/b] (" + Math.Truncate(item.Value * 1.5) + " after Salac Berry) ");
                    }
                    sw.WriteLine(":" + item.Key + ": " + item.Key);
                    currentSpeed = item.Value;
                }
            }

        }

        //gets a pokemon's base speed by calling pokeAPI
        static async Task<int> getPokemonBaseSpeed(string pokemonName)
        {
            int speed = 0;
            using (var httpClient = new HttpClient())
            {
                string url = "https://pokeapi.co/api/v2/pokemon/" + pokemonName;
                dynamic json = JsonConvert.DeserializeObject(await httpClient.GetStringAsync(url));
                speed = json.stats[5].base_stat;
            }
            return speed;
        }

        //calculcates a pokemon's speed stat
        static double calculateSpeed(double baseSpeed)
        {
            //formula taken from bulbapedia
            //((((2 * baseSpeed + iv + (ev / 4) * level) / 100) + 5)) * nature;
            double result;
            result = 2 * baseSpeed;
            result += maxSpeed.iv;
            result += (maxSpeed.ev / 4);
            result *= maxSpeed.level;
            result /= 100;
            result += 5;
            result = Math.Truncate(result);
            result *= maxSpeed.nature;

            return result;
        }
    }
}
