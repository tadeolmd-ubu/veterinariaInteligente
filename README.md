# veterinariaInteligente

Aplicación de escritorio para la gestión de una veterinaria usando Avalonia, .NET 10, Entity Framework Core 10, SQLite y CommunityToolkit.Mvvm.

## Requisitos

- SDK de .NET 10: https://dotnet.microsoft.com/download/dotnet/10.0
- Git
- Rider, Visual Studio o VS Code con soporte para .NET

## Clonar y preparar el proyecto

Con SSH:

```bash
git clone git@github.com:tadeolmd-ubu/veterinariaInteligente.git
```

Con HTTPS:

```bash
git clone https://github.com/tadeolmd-ubu/veterinariaInteligente.git
```

Entrar en la carpeta del repositorio y restaurar las dependencias:

```bash
cd veterinariaInteligente
dotnet tool restore
dotnet restore
```

Verificar las versiones principales:

```bash
dotnet --version
dotnet tool run dotnet-ef --version
```

La herramienta local `dotnet-ef` está fijada en la versión 10.0.12. Usar siempre `dotnet tool run dotnet-ef` en lugar de una instalación global.

## Crear la base local

El archivo SQLite se llama `veterinariaInteligente.db` y está excluido de Git. Cada desarrollador genera su propia copia aplicando las migraciones versionadas:

```bash
cd veterinariaInteligente
dotnet tool run dotnet-ef database update
```

No se debe commitear ningún archivo `.db`.

## Compilar, probar y ejecutar

Desde la raíz del repositorio:

```bash
dotnet build veterinariaInteligente.sln
dotnet test veterinariaInteligente.sln
dotnet run --project veterinariaInteligente/veterinariaInteligente.csproj
```

Si ya estás dentro de `veterinariaInteligente/`, podés usar:

```bash
dotnet build
dotnet run
```

## Estructura del proyecto

```text
veterinariaInteligente/
├── Data/                  DbContext, factory y configuración de SQLite
├── Models/                Entidades y enums del dominio
├── Migrations/            Historial versionado de cambios de la base
├── Service/               Servicios de aplicación
├── ViewModels/            ViewModels de Avalonia/MVVM
├── Views/                 Vistas XAML y code-behind
└── veterinariaInteligente.csproj

veterinariaInteligente.Tests/   Proyecto de pruebas
```

El contexto principal está en `veterinariaInteligente/Data/AppDbContext.cs`. La factory de diseño está en `Data/AppDbContextFactory.cs` y define la conexión usada por Entity Framework durante las migraciones.

## Entidades y relaciones actuales

Las entidades mapeadas actualmente son:

- `Duenio`
- `Profesional`
- `MascotaEstado`
- `Mascota`
- `Consulta`
- `ConsultaVersion`
- `Vacuna`
- `AplicacionVacuna`
- `Seguimiento`
- `Cita`
- `ConfiguracionCliente`

`Vacuna` es el catálogo de tipos de vacuna. `AplicacionVacuna` registra una aplicación concreta sobre una mascota.

Los estados de `Consulta`, `Seguimiento` y `Cita` son enums guardados como texto mediante `HasConversion<string>()`. `MascotaEstado` es una entidad independiente con su propia tabla y FK desde `Mascota`.

## Agregar o modificar una entidad

Para cada cambio de modelo, seguir este orden:

1. Crear o modificar la clase en `Models/Nombre.cs`.
2. Usar tipos nullable (`string?`, `int?`, `DateOnly?`, `decimal?`) cuando la columna permita `NULL`.
3. Agregar las propiedades de navegación y colecciones inversas cuando exista una relación.
4. Registrar el `DbSet` en `Data/AppDbContext.cs`:

   ```csharp
   public DbSet<Nombre> Nombres => Set<Nombre>();
   ```

5. Configurar la entidad dentro de `OnModelCreating`:
   - `ToTable` para el nombre de la tabla.
   - `HasKey` para la clave primaria.
   - `HasColumnName` y `HasMaxLength` para las columnas.
   - `IsRequired()` solo para columnas `NOT NULL`.
   - `HasOne().WithMany().HasForeignKey()` para relaciones.
   - `HasConversion<string>()` para enums almacenados como texto.
6. Compilar antes de generar la migración:

   ```bash
   dotnet build veterinariaInteligente.sln
   ```

7. Crear una migración por conjunto lógico de cambios, no necesariamente una por entidad:

   ```bash
   cd veterinariaInteligente
   dotnet tool run dotnet-ef migrations add NombreDescriptivo
   ```

8. Revisar el archivo generado en `Migrations/` antes de aplicarlo. Comprobar tablas, columnas, nullabilidad, longitudes, FKs, índices y que no haya eliminaciones inesperadas.
9. Aplicar la migración:

   ```bash
   dotnet tool run dotnet-ef database update
   ```

10. Ver las migraciones conocidas:

   ```bash
   dotnet tool run dotnet-ef migrations list
   ```

Si una migración se generó por error y todavía no fue aplicada, quitarla con:

```bash
dotnet tool run dotnet-ef migrations remove
```

No editar manualmente los archivos `.Designer.cs` ni `AppDbContextModelSnapshot.cs`; EF Core los genera y actualiza automáticamente.

## Verificar SQLite desde la terminal

Si `sqlite3` está instalado:

```bash
sqlite3 veterinariaInteligente.db ".tables"
sqlite3 veterinariaInteligente.db ".schema Consulta"
```

Sin instalar `sqlite3`, se puede inspeccionar el esquema básico con:

```bash
strings veterinariaInteligente.db | grep -A20 "CREATE TABLE"
```

## Flujo de ramas y commits

No trabajar directamente sobre `main`. Crear una rama desde la última versión:

```bash
git switch main
```

Prefijos recomendados para commits:

- `feat:` nueva funcionalidad o entidad
- `fix:` corrección de un error
- `docs:` cambios de documentación
- `test:` pruebas
- `build:` cambios de paquetes o compilación
- `chore:` tareas de mantenimiento

Ejemplo:

```bash
git add veterinariaInteligente/Models/MiEntidad.cs
git commit -m "feat: add MiEntidad model and mapping"
```

Antes de abrir un Pull Request, verificar:

```bash
dotnet build veterinariaInteligente.sln
```
