using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace speedtiergenerator
{
    class Pokemon
    {
        public string name;

        public double baseSpeed;
        public int iv;
        public int ev;
        public int level;
        public double nature;

        public double calculatedSpeed;
        public double speedStage;

        public Pokemon(string pokemonName, double pokemonBaseSpeed, int pokemonIv, int pokemonEv, int pokemonLevel, double pokemonNature, double pokemonCalculatedSpeed, double pokemonSpeedStage)
        {
            name = pokemonName;
            baseSpeed = pokemonBaseSpeed;
            iv = pokemonIv;
            ev = pokemonEv;
            level = pokemonLevel;
            nature = pokemonNature;
            calculatedSpeed = pokemonCalculatedSpeed;
            speedStage = pokemonSpeedStage;
        }
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

            //create list of pokemon
            //string pokemonName, double pokemonBaseSpeed, int pokemonIv, int pokemonEv, int pokemonLevel, double pokemonNature, double pokemonCalculatedSpeed, double pokemonSpeedStage
            createPokemonList(splitPokemon);
            Console.ReadKey();
        }

        static async void createPokemonList(string[] splitPokemon)
        {
            List<Pokemon> pokemonList = new List<Pokemon>();
            foreach (var pokemon in splitPokemon)
            {
                int baseSpeed = await getPokemonBaseSpeed(pokemon.ToLower());
                //sets pokemon up to have max speed investment
                Pokemon tempPokemon = new Pokemon(pokemon, baseSpeed, 31, 252, 5, 1.1, 0, 1);
                tempPokemon.calculatedSpeed = calculateSpeed(tempPokemon);
                pokemonList.Add(tempPokemon);
            }
            generateOutput(pokemonList);
            Console.WriteLine("Finished, press any key to exit");
        }

        //formats the speed tier post and writes it to output.txt
        static void generateOutput(List<Pokemon> pokemonList)
        {
            //sort the list by speed value
            pokemonList = pokemonList.OrderByDescending(x => x.calculatedSpeed).ToList();
            using (StreamWriter sw = new StreamWriter("output.txt"))
            {
                double currentSpeed = 0;
                foreach (var pokemon in pokemonList)
                {
                    //get current speed
                    //check if its the same as last speed
                    if (currentSpeed != pokemon.calculatedSpeed)
                    {
                        sw.WriteLine();
                        //strings for ADV LC's simplified speed tier document
                        sw.WriteLine("[b]" + pokemon.calculatedSpeed * 2 + " speed[/b] (at +2 due to ability or Agility)");
                        sw.WriteLine("[b]" + pokemon.calculatedSpeed + " speed[/b] (" + Math.Truncate(pokemon.calculatedSpeed * 1.5) + " after Salac Berry) ");
                    }
                    sw.WriteLine();
                    //for ADV LC
                    sw.WriteLine(":" + pokemon.name + ": " + pokemon.name);
                    currentSpeed = pokemon.calculatedSpeed;
                }
            }

        }

        //calculate a pokemon's speed stat
        static double calculateSpeed(Pokemon pokemon)
        {
            //formula taken from bulbapedia
            //((((2 * baseSpeed + iv + (ev / 4) * level) / 100) + 5)) * nature;
            double result;
            result = 2 * pokemon.baseSpeed;
            result += pokemon.iv;
            result += (pokemon.ev / 4);
            result *= pokemon.level;
            result /= 100;
            result += 5;
            result = Math.Truncate(result);
            result *= pokemon.nature;

            return result;
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
    }
}
