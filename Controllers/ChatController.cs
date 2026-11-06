using InternshipManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagementSystem.Controllers
{
    // No special route attributes needed! It will use the default MVC routing.
    public class ChatController : Controller
    {
        private readonly GeminiService _geminiService;

        public ChatController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message cannot be empty.");
            }

            // We add a "System Prompt" so the AI knows exactly what its job is
            string fullPrompt = $"You are a helpful, professional AI assistant for HireNest, an Internship Management System. Answer the following user question concisely: {message}";

            var response = await _geminiService.AskAIAsync(fullPrompt);

            return Ok(new { reply = response });
        }
    }
}