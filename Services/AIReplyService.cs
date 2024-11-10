using OpenAI;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace SupportTicketSystem.Services
{
    public class AIReplyService
    {
        private readonly OpenAIClient _openAIClient;

        // Constructor to initialize OpenAIClient with API Key
        public AIReplyService(string apiKey)
        {
            // Initialize OpenAIClient with the API key directly in the constructor
            _openAIClient = new OpenAIClient(apiKey); // No need for OpenAIClientOptions
        }

        // Method to get a suggested reply for a ticket based on the ticket's content
        public async Task<string> GetSuggestedReply(string ticketContent)
        {
            // Construct the prompt for the response
            string prompt = $"You are a helpful support agent. Suggest a response for the following ticket:\n{ticketContent}";

            // Create a ChatMessage as per the new SDK (chat completions are recommended for better conversations)
            var chatMessage = new ChatMessage
            {
                Role = "system",
                Content = prompt
            };

            // Create a request object for chat-based completion
            var chatRequest = new ChatRequest
            {
                Model = "gpt-3.5-turbo", // Example model, update based on your availability
                Messages = new[] { chatMessage }
            };

            // Send the request using the chat completions API
            var chatResponse = await _openAIClient.ChatCompletions.CreateCompletionAsync(chatRequest);

            // Return the response content (ensure that 'Choices' is available in the result)
            return chatResponse.Choices[0].Message.Content.Trim();
        }
    }
}
