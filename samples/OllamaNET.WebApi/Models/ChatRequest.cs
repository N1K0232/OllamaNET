namespace OllamaNET.WebApi.Models;

public record class ChatRequest(Guid ConversationId, string Message);