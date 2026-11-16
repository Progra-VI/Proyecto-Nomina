using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Nomina_back_end.Repositories;

// Almacenamiento de archivos (comprobantes PDF) en GridFS: colecciones comprobantes.files y comprobantes.chunks
public class ComprobanteStorage
{
    private readonly GridFSBucket _bucket;

    public ComprobanteStorage(IMongoDatabase db)
    {
        _bucket = new GridFSBucket(db, new GridFSBucketOptions { BucketName = "comprobantes" });
    }

    public async Task<ObjectId> Guardar(string nombre, byte[] contenido, string liquidacionId) =>
        await _bucket.UploadFromBytesAsync(nombre, contenido, new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "tipo", "liquidacion" },
                { "liquidacion_id", liquidacionId },
                { "content_type", "application/pdf" }
            }
        });

    public async Task<byte[]?> Descargar(string id)
    {
        if (!ObjectId.TryParse(id, out var oid)) return null;

        try { return await _bucket.DownloadAsBytesAsync(oid); }
        catch (GridFSFileNotFoundException) { return null; }
    }

    public async Task Eliminar(ObjectId id)
    {
        try { await _bucket.DeleteAsync(id); }
        catch { /* si no existe no pasa nada */ }
    }
}