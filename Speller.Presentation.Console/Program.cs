using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Speller.SpellingBox.Services;

namespace Speller.Presentation.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Execute();   
        }

        static async void Execute()
        {
            // Instantiate the spelling service
            SpellingService spellingService = new SpellingService();

            // Add words to the dictionary
            spellingService.AddDictionary(
                new List<string>()
                {
                    "divorciado",
                    "solteiro",
                    "casado",
                    "viúvo"
                }
            );

            string wordSuggestion = spellingService.SuggestCorrection("devorciado");

            Task<List<string>> wordsSuggestion = spellingService.SuggestCorrection(
                new List<string>()
                {
                    "casada",
                    "solteira",
                    "divorciado(a)"
                }
            );

            wordsSuggestion.Wait();

            System.Console.WriteLine(wordsSuggestion.ToString());
            System.Console.ReadKey();
        }
    }
}
