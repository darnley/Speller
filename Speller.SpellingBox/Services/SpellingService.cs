using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Speller.SpellingBox.Services
{
    /// <summary>
    /// Implement the SpellingService interface.
    /// It is supposed to be used by a dependency injector.
    /// </summary>
    public interface ISpellingService
    {
        void AddDictionary(IEnumerable<string> dictionary);
        string SuggestCorrection(string word);
        Task<List<string>> SuggestCorrection(List<string> words);
    }

    /// <summary>
    /// Implement the SpellingService class.
    /// You SHOULD implement this using a dependency injector.
    /// </summary>
    public class SpellingService : ISpellingService
    {
        public IMachineLearningService _machineLearningService;

        /// <summary>
        /// The symmetric delete spelling algorithm object.
        /// </summary>
        private SymSpell _symSpellInstance { get; set; }

        /// <summary>
        /// The maximum memory that can be allocated by the algorithm.
        /// </summary>
        public readonly long maximumMemorySize = GC.GetTotalMemory(true);

        /// <summary>
        /// The initial memory capacity in bytes for memory allocation.
        /// </summary>
        public readonly int initialCapacity = 82765;

        /// <summary>
        /// Control up to which edit distance words from the dictionary should be treated as suggestions.
        /// </summary>
        public readonly int maximumEditDistance = 4;

        /// <summary>
        /// Control the block length for dictionary precalculation.
        /// </summary>
        public readonly int prefixLength = 10;

        /// <summary>
        /// Implement the dependency injection.
        /// </summary>
        public SpellingService()
        {
            this._symSpellInstance = new SymSpell(this.initialCapacity, this.maximumEditDistance, this.prefixLength);
        }

        /// <summary>
        /// Implement the dependency injection.
        /// </summary>
        public SpellingService(IMachineLearningService machineLearningService)
        {
            this._machineLearningService = machineLearningService;
            this._symSpellInstance = new SymSpell(this.initialCapacity, this.maximumEditDistance, this.prefixLength);
        }

        /// <summary>
        /// Add some words to the dictionary.
        /// </summary>
        /// <param name="dictionary">The words to be used as dictionary</param>
        public void AddDictionary(IEnumerable<string> dictionary)
        {
            Parallel.ForEach(dictionary, (currentWord) =>
            {
                // Add each word to be recognized as dictionary
                _symSpellInstance.CreateDictionaryEntry(currentWord, 1);
            });
        }

        /// <summary>
        /// Suggest a correction for a word.
        /// </summary>
        /// <param name="word">The word to be verified.</param>
        /// <returns>The top rated word by the algorithm.</returns>
        public string SuggestCorrection(string word)
        {
            // It removes all special characters using RegEx replacing
            var suggestion = _symSpellInstance.Lookup(RemoveSpecialCharacters(word), SymSpell.Verbosity.Top).FirstOrDefault();

            if (suggestion == null)
            {
                // There is NO suggestion for this word
                return null;
            }

            // There is a suggestion for this word
            return suggestion.term;
        }

        /// <summary>
        /// Suggest a correction for some words in a list.
        /// </summary>
        /// <param name="words">The words to be verified.</param>
        /// <returns>The top rated words by the algorithm.</returns>
        public Task<List<string>> SuggestCorrection(List<string> words)
        {
            return Task.Run(() =>
            {
                Parallel.For(0, words.Count(), (currentWordIndex) =>
                {
                    string valueToAdd = this.SuggestCorrection(words[currentWordIndex]);

                    if (valueToAdd == null)
                    {
                        // Add the word to the ML list to be sent later
                        this._machineLearningService.AddWord(
                            new Models.MachineLearningWord() {
                                Word = words[currentWordIndex],
                                Index = currentWordIndex
                            });
                    } else
                    {
                        words[currentWordIndex] = valueToAdd;
                    }
                });

                return words;
            });
        }

        /// <summary>
        /// Remove special characters from string.
        /// It leave only alphanumeric characters.
        /// </summary>
        /// <param name="str">The string to apply the replacing.</param>
        /// <returns>The string without non-alphanumeric characters.</returns>
        private static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
    }
}
