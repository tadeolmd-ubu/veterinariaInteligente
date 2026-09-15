# veterinariaInteligente

Aplicación de escritorio para la gestión de una veterinaria usando Avalonia, .NET 10, Entity Framework Core 10 con SQLite y CommunityToolkit.Mvvm.

## Requisitos

- SDK de .NET 10 (https://dotnet.microsoft.com/download/dotnet/10.0)

## Clonar el repositorio

```bash
git clone git@github.com:tadeolmd-ubu/veterinariaInteligente.git
cd veterinariaInteligente/veterinariaInteligente
```

## Configuración inicial (primera vez)

Restaurar la herramienta local de Entity Framework Core (`dotnet-ef`):

```bash
dotnet tool restore
```

Restaurar los paquetes NuGet del proyecto:

```bash
dotnet restore
```

Verificar que `dotnet-ef` quedó instalado correctamente:

```bash
dotnet tool run dotnet-ef --version
```

## Compilar y ejecutar

```bash
dotnet build
dotnet run
```

## Migraciones de base de datos

Cuando el modelo de datos esté definido, crear y aplicar migraciones desde la carpeta del proyecto:

```bash
dotnet tool run dotnet-ef migrations add <NombreMigracion>
dotnet tool run dotnet-ef database update
```

Usa siempre `dotnet tool run dotnet-ef` (no un `dotnet-ef` global) para que todos trabajen con la misma versión del equipo.

## Cómo agregar una entidad a la base de datos (paso a paso)

Cada vez que se agrega o modifica una entidad:

1. **Crear el modelo** en `veterinariaInteligente/Models/Nombre.cs` (POCO: propiedades públicas, `string?` para columnas NULL, `DateOnly?` para fechas).
2. **Registrar el `DbSet`** en `veterinariaInteligente/Data/AppDbContext.cs`:
   ```csharp
   public DbSet<Nombre> Nombres => Set<Nombre>();
   ```
3. **Configurar en `OnModelCreating`**: `ToTable`, `HasKey`, columnas (`HasColumnName` + `HasMaxLength` + `IsRequired()`), FKs (`HasOne().WithMany()`) e índices (`HasIndex`).
4. **Generar la migración** (una por conjunto de cambios, no por entidad):
   ```bash
   dotnet tool run dotnet-ef migrations add NombreDescriptivo
   ```
5. **Aplicarla** (crea/actualiza el archivo `veterinariaInteligente.db`, que está en `.gitignore`):
   ```bash
   dotnet tool run dotnet-ef database update
   ```

### Verificar el esquema creado

Si `sqlite3` no está instalado, desde la carpeta del proyecto:

```bash
strings veterinariaInteligente.db | grep -A20 "CREATE TABLE"
```