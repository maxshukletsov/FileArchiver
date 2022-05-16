using System.IO.Compression;
using Google.Protobuf;
using Grpc.Core;

namespace Core.Services;


public class ArchiverService : Archiver.ArchiverBase
{
    private readonly ILogger<ArchiverService> _logger;

    public ArchiverService(ILogger<ArchiverService> logger)
    {
        _logger = logger;
    }

    public async override Task<ArchiveReply> CreateArchive(ArchiveRequest request, ServerCallContext context)
    {
        var archive = await CreateZipArhive(request.Document.ToList());
        return new ArchiveReply
        {
            Archive = archive
        };
    }
    private async Task<ByteString> CreateZipArhive(List<UserDocument> files)
    {
        await using var outStream = new MemoryStream();
        using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var documentInArchive = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                documentInArchive.ExternalAttributes = Convert.ToInt32("664", 8) << 16;
                var byteArray = file.File.ToByteArray();
                await using (var documentStream = new MemoryStream(byteArray))
                {
                    await using (var entryStream = documentInArchive.Open())
                    {
                        documentStream.Position = 0;
                        await documentStream.CopyToAsync(entryStream);
                    }
                }
            }
        }

        return ByteString.CopyFrom(outStream.ToArray());
    }
}