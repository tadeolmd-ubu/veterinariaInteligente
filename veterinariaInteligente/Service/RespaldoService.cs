using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;

namespace veterinariaInteligente.Service;

public class RespaldoService
{
    private readonly AppDbContext _db;

    public RespaldoService(AppDbContext db) => _db = db;

    public async Task<Resultado<string>> CrearRespaldoAsync(
        string carpetaDestino)
    {
        if (string.IsNullOrWhiteSpace(carpetaDestino))
        {
            return new Resultado<string>(
                false,
                "La carpeta de destino es obligatoria.",
                null);
        }

        string? rutaTemporal = null;

        try
        {
            var cadenaConexion = _db.Database.GetConnectionString();

            if (string.IsNullOrWhiteSpace(cadenaConexion))
            {
                return new Resultado<string>(
                    false,
                    "No se encontró la conexión de la base de datos.",
                    null);
            }

            var opcionesOrigen = new SqliteConnectionStringBuilder(cadenaConexion);

            if (opcionesOrigen.Mode == SqliteOpenMode.Memory ||
                opcionesOrigen.DataSource == ":memory:" ||
                string.IsNullOrWhiteSpace(opcionesOrigen.DataSource))
            {
                return new Resultado<string>(
                    false,
                    "El respaldo requiere una base de datos guardada en disco.",
                    null);
            }

            opcionesOrigen.Mode = SqliteOpenMode.ReadOnly;
            opcionesOrigen.Pooling = false;

            var carpeta = Path.GetFullPath(carpetaDestino.Trim());
            Directory.CreateDirectory(carpeta);

            var identificador = Guid.NewGuid().ToString("N");

            var nombreArchivo =
                $"respaldo_{DateTime.Now:yyyyMMdd_HHmmss}_{identificador}.db";

            var rutaDestino = Path.Combine(carpeta, nombreArchivo);

            rutaTemporal = Path.Combine(
                carpeta,
                $".respaldo_{identificador}.tmp");

            var opcionesDestino = new SqliteConnectionStringBuilder
            {
                DataSource = rutaTemporal,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false
            };

            await using (var origen = new SqliteConnection(opcionesOrigen.ToString()))
            {
                await using var destino = new SqliteConnection(opcionesDestino.ToString());

                await origen.OpenAsync();
                await destino.OpenAsync();

                origen.BackupDatabase(destino);
            }

            File.Move(rutaTemporal, rutaDestino);
            rutaTemporal = null;

            return new Resultado<string>(
                true,
                "Respaldo creado.",
                rutaDestino);
        }
        catch (SqliteException)
        {
            return new Resultado<string>(
                false,
                "No se pudo respaldar la base de datos.",
                null);
        }
        catch (UnauthorizedAccessException)
        {
            return new Resultado<string>(
                false,
                "No hay permisos para escribir en la carpeta seleccionada.",
                null);
        }
        catch (IOException)
        {
            return new Resultado<string>(
                false,
                "No se pudo escribir el archivo de respaldo.",
                null);
        }
        catch (ArgumentException)
        {
            return new Resultado<string>(
                false,
                "La ruta de destino no es válida.",
                null);
        }
        catch (NotSupportedException)
        {
            return new Resultado<string>(
                false,
                "El formato de la ruta de destino no es compatible.",
                null);
        }
        finally
        {
            if (rutaTemporal is not null)
                EliminarTemporal(rutaTemporal);
        }
    }

    private static void EliminarTemporal(string ruta)
    {
        try
        {
            File.Delete(ruta);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}