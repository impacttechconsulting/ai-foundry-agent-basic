using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace AiFoundryAgent.Services;

public interface IDocumentProcessingService
{
    Task<List<string>> ProcessAndChunkDocumentAsync(string blobName, BlobContainerClient blobContainerClient);
    List<string> ChunkText(string content, int maxChunkSize = 1000, int overlap = 100);
}

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly ILogger<DocumentProcessingService> _logger;
    private readonly IEmbeddingService _embeddingService;

    public DocumentProcessingService(ILogger<DocumentProcessingService> logger, IEmbeddingService embeddingService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
    }

    public async Task<List<string>> ProcessAndChunkDocumentAsync(string blobName, BlobContainerClient blobContainerClient)
    {
        try
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            
            // Download the blob content
            var response = await blobClient.DownloadContentAsync();
            var content = response.Value.Content.ToString();
            
            // Extract text based on file extension
            var extension = Path.GetExtension(blobName).ToLowerInvariant();
            string extractedText;
            
            switch (extension)
            {
                case ".txt":
                    extractedText = content;
                    break;
                case ".pdf":
                    extractedText = ExtractTextFromPdf(content, blobClient);
                    break;
                case ".docx":
                    extractedText = await ExtractTextFromDocxAsync(blobClient);
                    break;
                case ".doc":
                    _logger.LogWarning("DOC format not directly supported, processing as text");
                    extractedText = content;
                    break;
                default:
                    // For now, treat all other formats as text
                    extractedText = content;
                    break;
            }
            
            // Chunk the extracted text
            var chunks = ChunkText(extractedText, maxChunkSize: 1000, overlap: 100);
            
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing document {BlobName}", blobName);
            throw;
        }
    }
    
    private string ExtractTextFromPdf(string content, BlobClient blobClient)
    {
        // For PDF processing, we'd typically use a library like iTextSharp or PDFsharp
        // Since those libraries aren't currently in the project, we'll return the raw content
        // In a real implementation, you would install the necessary NuGet package
        _logger.LogWarning("PDF text extraction requires additional libraries. Returning raw content for now.");
        return content;
    }
    
    private async Task<string> ExtractTextFromDocxAsync(BlobClient blobClient)
    {
        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream);
        memoryStream.Position = 0;
        
        using var docxPackage = WordprocessingDocument.Open(memoryStream, false);
        var text = new StringBuilder();
        
        var paragraphs = docxPackage.MainDocumentPart.Document.Elements<Paragraph>();
        foreach (var paragraph in paragraphs)
        {
            text.AppendLine(paragraph.InnerText);
        }
        
        return text.ToString();
    }

    public List<string> ChunkText(string content, int maxChunkSize = 1000, int overlap = 100)
    {
        if (string.IsNullOrEmpty(content))
        {
            return new List<string>();
        }

        var chunks = new List<string>();
        var paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = new StringBuilder();
        
        foreach (var paragraph in paragraphs)
        {
            // If adding the next paragraph would exceed the max chunk size
            if (currentChunk.Length + paragraph.Length > maxChunkSize && currentChunk.Length > 0)
            {
                // Add the current chunk to the list
                chunks.Add(currentChunk.ToString().Trim());
                
                // Start a new chunk with overlap from the previous chunk
                if (overlap > 0)
                {
                    var lastChunk = currentChunk.ToString();
                    var overlapStart = Math.Max(0, lastChunk.Length - overlap);
                    currentChunk = new StringBuilder(lastChunk.Substring(overlapStart));
                }
                else
                {
                    currentChunk = new StringBuilder();
                }
            }
            
            currentChunk.AppendLine(paragraph);
            
            // If the current chunk is still too large, split it into sentences
            if (currentChunk.Length > maxChunkSize)
            {
                var sentences = currentChunk.ToString().Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                currentChunk = new StringBuilder();
                
                foreach (var sentence in sentences)
                {
                    if (currentChunk.Length + sentence.Length + 1 > maxChunkSize && currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk = new StringBuilder();
                    }
                    
                    currentChunk.Append(sentence.Trim() + ". ");
                }
                
                // Add any remaining content in currentChunk
                if (currentChunk.Length > 0 && currentChunk.Length < maxChunkSize)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk = new StringBuilder();
                }
            }
        }
        
        // Add the final chunk if it has content
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }
        
        // Filter out any empty or whitespace-only chunks
        return chunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk)).ToList();
    }
}