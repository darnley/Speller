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
        /// <summary>
        /// Machine Learning Endpoint information
        /// It store the URI and API key
        /// </summary>
        private MachineLearningEndpoint _machineLearningEndpoint;

        /// <summary>
        /// The words to be sent to the Machine Learning Endpoint
        /// </summary>
        public List<MachineLearningWord> Words { get; set; }

        /// <summary>
        /// Initialize the basic objects
        /// </summary>
        public MachineLearningService()
        {
            this.Words = new List<MachineLearningWord>();
        }

        /// <summary>
        /// Initialize the basic objects and configure the Machine Learning Endpoint
        /// </summary>
        /// <param name="endpointUrl">The Machine Learning Endpoint API URL</param>
        /// <param name="apiKey">The Machine Learning Endpoint API key</param>
        public MachineLearningService(Uri endpointUrl, string apiKey) : this()
        {
            this.AddEndpoint(endpointUrl, apiKey);
        }

        /// <summary>
        /// Change the Machine Learning Endpoint information
        /// </summary>
        /// <param name="endpointUrl">The Machine Learning Endpoint API URL</param>
        /// <param name="apiKey">The Machine Learning Endpoint API key</param>
        public void AddEndpoint(Uri endpointUrl, string apiKey)
        {
            this._machineLearningEndpoint = new MachineLearningEndpoint()
            {
                Url = endpointUrl,
                ApiKey = apiKey
            };
        }

        /// <summary>
        /// Add a new word to the Machine Learning verification list
        /// </summary>
        /// <param name="word">The word to be added</param>
        public void AddWord(MachineLearningWord word) => this.Words.Add(word);

        /// <summary>
        /// Attempt the correction of the word list
        /// </summary>
        /// <param name="source">The list from which the words came</param>
        /// <returns>The task</returns>
        public Task CorrectWordsByIndexAsync(List<string> source)
        {
            return Task.Run(async () =>
            {
                // Send data to the API
                HttpResponseMessage response = await this.GetDataFromApi(this.Words.Select(s => s.Word));

                string responseRead = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Bind the returned JSON to the related class
                    MachineLearningResponse machineLearningResponse = JsonConvert.DeserializeObject<MachineLearningResponse>(responseRead);

                    // The source list will be changed by reference
                    try
                    {
                        this.PutCorrectionsIntoSourceList(source, machineLearningResponse.Results.Output);
                    }
                    catch (ArgumentNullException)
                    {
                        throw new InvalidCastException("Machine Learning return format is invalid");
                    }
                    
                } else
                {
                    throw new HttpRequestException("Machine Learning Endpoint is unavailable. Status Code: " + response.StatusCode.ToString());
                }
            });
        }

        /// <summary>
        /// Put the returned words into the source list
        /// The source list will be changed by memory reference to decrease memory usage
        /// </summary>
        /// <param name="source">The list from which the words came</param>
        /// <param name="correction">The list that contains the corrections from Machine Learning</param>
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

        /// <summary>
        /// Attempt a request to the Machine Learning API
        /// </summary>
        /// <param name="words">The words to be sent</param>
        /// <returns>The response from API</returns>
        private async Task<HttpResponseMessage> GetDataFromApi(IEnumerable<string> words)
        {
            MachineLearningRequest request = new MachineLearningRequest();

            // Construct the structure for server-side binding
            request.Inputs.Input1 = words.ToList();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this._machineLearningEndpoint.ApiKey);
                client.BaseAddress = new Uri($"{this._machineLearningEndpoint.Url.Scheme}://{this._machineLearningEndpoint.Url.Host}");

                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                return await client.PostAsync(this._machineLearningEndpoint.Url.AbsolutePath, stringContent);
            }
        }

        /// <summary>
        /// Verify if the word list have values
        /// </summary>
        /// <returns>A boolean result</returns>
        public bool HasIndexes() => this.Words.Count != 0 ? true : false;
    }
}
