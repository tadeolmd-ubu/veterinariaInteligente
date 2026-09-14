# veterinariaInteligente

Aplicación de escritorio para la gestión de una veterinaria usando Avalonia, .NET 8, Entity Framework Core 8 con SQLite y CommunityToolkit.Mvvm.

## Requisitos

- SDK de .NET 8 (https://dotnet.microsoft.com/download/dotnet/8.0)

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