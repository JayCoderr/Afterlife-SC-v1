using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using MelonLoader;
using Il2CppSystem.Net.Http;
using static _afterlifeScModMenu._globalVariables;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;

namespace _clientids
{
    internal class _airesponses
    {
        // Flag to ensure the responses are only fetched once
        private static bool responsesFetched = false;

        public static async Task FetchScheduleIResponses()
        {
            if (responsesFetched) return;  // If already fetched, don't fetch again

            try
            {
                string url = "https://raw.githubusercontent.com/JayCoderr/Schedule_I_Public_AI_Responses/main/ScheduleAIResponses.csv";
                var response = httpClient.GetAsync(url).Result;
                string content = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                string[] lines = content.Split('\n');
                foreach (var line in lines)
                {
                    // Trim the line to avoid any leading or trailing spaces/newlines
                    string trimmedLine = line.Trim();

                    if (trimmedLine.Contains("####"))
                    {
                        // Split the line into 3 parts: user input, ai name, and ai responses
                        string[] parts = trimmedLine.Split(new[] { "####" }, StringSplitOptions.None);

                        if (parts.Length == 4)
                        {
                            string userInput = parts[0].Trim().ToLowerInvariant();
                            string aiName = parts[1].Trim().ToLowerInvariant();
                            string aiActionType = parts[2].Trim().ToLowerInvariant();
                            string aiResponses = parts[3].Trim();

                            // Replace the {newline} placeholder with actual newlines
                            aiResponses = aiResponses.Replace("{newline}", "\n");

                            // Split aiResponses by &&& to handle multiple responses
                            string[] responses = aiResponses.Split(new[] { "&&&" }, StringSplitOptions.RemoveEmptyEntries);
                            List<string> responsesList = new List<string>();

                            foreach (var responseX in responses)
                            {
                                string cleanResponse = responseX.Trim();
                                if (!string.IsNullOrEmpty(cleanResponse))
                                    responsesList.Add(cleanResponse);
                            }

                            // Store the responses in the dictionary using the combined key
                            if (responsesList.Count > 0)
                            {
                                string combinedKey = $"{userInput}####{aiName}####{aiActionType}";
                                responseMap[combinedKey] = responsesList;
                            }
                        }
                        else
                        {
                            _afterlifeConsole($"Skipping line (invalid format): {trimmedLine}");
                        }
                    }
                }

                // Mark as fetched
                responsesFetched = true;
            }
            catch (Exception e)
            {
                _afterlifeConsole($"Error fetching CSV: {e.Message}");
            }
        }

        public static async Task<string> GetScheduleIAIResponse(string requestedUserInput, string aiName, string aiActionType)
        {
            // Ensure CSV is fetched and parsed before trying to get a response
            await FetchScheduleIResponses();

            // Construct the combined key
            string combinedKey = $"{requestedUserInput}####{aiName}####{aiActionType}";

            // Log the combined key for debugging purposes
            _afterlifeConsole($"Looking for key: {combinedKey}");

            // Look for the key in the response map
            if (responseMap.TryGetValue(combinedKey, out var responses))
            {
                // Select and return only one response
                string chosenResponse = responses[random.Next(responses.Count)];
                _afterlifeConsole($"{aiName}: {chosenResponse}");
                return chosenResponse; // Return the response
            }
            else
            {
                _afterlifeConsole($"No AI responses found for: {requestedUserInput} with AI: {aiName} and {aiActionType}");
                return null; // Return null if no response found
            }
        }
    }
}
