using Speller.SpellingBox.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Speller.SpellingBox.Services
{
    public interface IMachineLearningService
    {
        void AddIndex(int index);
        Task CorrectWordsByIndexAsync(List<string> source);
        bool HasIndexes();
    }

    public class MachineLearningService : IMachineLearningService
    {
        public ICollection<int> WordIndexes { get; set; }

        public MachineLearningService()
        {
            this.WordIndexes = new List<int>();
        }

        public void AddIndex(int index) => this.WordIndexes.Add(index);

        public Task CorrectWordsByIndexAsync(List<string> source)
        {
            return Task.Run(() =>
            {
                if (this.WordIndexes.Count == 0 || source.Count == 0)
                {
                    return;
                }

                Parallel.ForEach(WordIndexes, (currentWordIndex) =>
                {
                    ///TODO: Call API using source[currentWordIndex]

                    source[currentWordIndex] = string.Empty;
                });
            });
        }

        public bool HasIndexes() => this.WordIndexes.Count != 0 ? true : false;
    }
}
