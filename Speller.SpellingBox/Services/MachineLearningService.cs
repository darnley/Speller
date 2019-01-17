using Newtonsoft.Json;
using Speller.SpellingBox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Speller.SpellingBox.Services
{
    public interface IMachineLearningService
    {
        void AddEndpoint(Uri endpointUrl, string apiKey);
        void AddWord(MachineLearningWord word);
        Task CorrectWordsByIndexAsync(List<string> source);
        bool HasIndexes();
    }

    public class MachineLearningService : IMachineLearningService
    {
        private MachineLearningEndpoint _machineLearningEndpoint;

        public List<MachineLearningWord> Words { get; set; }

        public MachineLearningService()
        {
            this.Words = new List<MachineLearningWord>();
        }

        public MachineLearningService(Uri endpointUrl, string apiKey) : this()
        {
            this.AddEndpoint(endpointUrl, apiKey);
        }

        public void AddEndpoint(Uri endpointUrl, string apiKey)
        {
            this._machineLearningEndpoint = new MachineLearningEndpoint()
            {
                Url = endpointUrl,
                ApiKey = apiKey
            };
        }

        public void AddWord(MachineLearningWord word) => this.Words.Add(word);

        public Task CorrectWordsByIndexAsync(List<string> source)
        {
            return Task.Run(async () =>
            {
                if (this.Words.Count == 0 || source.Count == 0)
                {
                    return;
                }
                
                var request = new MachineLearningRequest();

                request.Inputs.Input1 = this.Words.Select(s => s.Word).ToList<string>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this._machineLearningEndpoint.ApiKey);
                    client.BaseAddress = new Uri($"{this._machineLearningEndpoint.Url.Scheme}://{this._machineLearningEndpoint.Url.Host}");

                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(this._machineLearningEndpoint.Url.AbsolutePath, stringContent);

                    string responseRead = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        MachineLearningResponse machineLearningResponse = JsonConvert.DeserializeObject<MachineLearningResponse>(responseRead);

                        this.PutCorrectionsIntoSourceList(source, machineLearningResponse.Results.Output);
                        // Success
                    } else
                    {
                        throw new HttpRequestException("Machine Learning Endpoint is unavailable. Status Code: " + response.StatusCode.ToString());
                    }
                }
            });
        }

        private void PutCorrectionsIntoSourceList(List<string> source, List<string> correction)
        {
            Parallel.For(0, correction.Count(), (currentIndex) =>
            {
                // Get the index based on Azure ML response word index
                // We assume that the response and list are in the same order
                int sourceListIndex = this.Words[currentIndex].Index;

                // Edit the source list by index putting the correct word
                source[sourceListIndex] = correction[currentIndex];
            });
        }

        public bool HasIndexes() => this.Words.Count != 0 ? true : false;
    }
}
