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

## Publicación y actualizaciones automáticas (Windows x64)

El workflow `.github/workflows/deploy.yml` ejecuta las pruebas y publica una
release con cada push a `main`. También se puede ejecutar manualmente desde
GitHub Actions. La versión sigue el formato `1.0.N`, donde `N` es el número
de ejecución del workflow. Un merge a `main` también dispara la publicación.
La nueva release se publica únicamente si las pruebas y la generación del
paquete terminan correctamente.

### Primera instalación en Windows 11 (x64)

1. Entrar en [GitHub Releases](https://github.com/tadeolmd-ubu/veterinariaInteligente/releases).
2. En la última versión, abrir **Assets** y descargar
   `veterinariaInteligente-1.0.N-win-x64.zip`, donde `N` corresponde a la versión
   publicada. No descargar los archivos **Source code**.
3. Si se descargó desde otra computadora, transferir el ZIP a Windows.
4. Hacer clic derecho sobre el ZIP y seleccionar **Extraer todo**. Usar una
   carpeta con permisos de escritura, por ejemplo `C:\Users\TU_USUARIO\Apps\PawDesk`.
5. Abrir `veterinariaInteligente.exe` desde la carpeta extraída.

Conservar todos los archivos de la carpeta: el ejecutable necesita las bibliotecas
incluidas en el paquete. No hace falta instalar .NET, porque su runtime viene
incluido. Ejecutar la aplicación desde la carpeta extraída, no desde el ZIP.

Si todavía no aparece el paquete, revisar la ejecución en
[Actions → Build and Release](https://github.com/tadeolmd-ubu/veterinariaInteligente/actions/workflows/deploy.yml).

### Cuándo aparece el aviso de actualización

En compilaciones Release para Windows, la aplicación consulta las releases
públicas de `tadeolmd-ubu/veterinariaInteligente` al iniciar. Si hay una versión
nueva, muestra un diálogo con **Más tarde** y **Actualizar**. Al aceptar, descarga
el paquete, cierra la aplicación, reemplaza los archivos y vuelve a abrirla.

El flujo completo es:

```text
Push o merge a main
  → pruebas y generación del ejecutable en GitHub Actions
  → publicación de la nueva release
  → el usuario abre una versión anterior de la aplicación
  → aviso de actualización
  → descarga, instalación y reinicio si acepta
```

El aviso no llega inmediatamente a una aplicación que ya está abierta: se
consulta al iniciar. Si el usuario elige **Más tarde**, podrá recibir el aviso
en el próximo inicio mientras siga instalada una versión anterior.

Para recibir el aviso, el usuario debe tener una versión que incluya el
actualizador, conexión a Internet y acceso a las releases públicas. Si no se
puede consultar GitHub, la aplicación continúa funcionando. Las compilaciones
Debug no consultan actualizaciones.

### Configuración en GitHub

El workflow usa el `GITHUB_TOKEN` automático de GitHub y declara el permiso
`contents: write` para publicar releases; no requiere crear un token o secreto
adicional. GitHub Actions debe estar habilitado y las políticas del repositorio
deben permitir las acciones utilizadas y la creación de releases.

El actualizador de escritorio accede sin autenticación a las releases. Un
repositorio privado requiere otro mecanismo de acceso para el cliente.

### Base de datos y respaldos

En Release, SQLite se guarda en la carpeta de datos local del usuario:
`%LOCALAPPDATA%\VeterinariaInteligente\veterinariaInteligente.db` en Windows.
Si existe una base junto al ejecutable y aún no hay una en la nueva ubicación,
se importa mediante un respaldo SQLite, conservando el archivo original.
En Debug se conserva la búsqueda de la base dentro del proyecto.

Al iniciar se aplican las migraciones pendientes. Si la base ya existe,
se crea antes un respaldo en la subcarpeta `Respaldos`; si falla el respaldo,
no se ejecuta la migración. Los paquetes de actualización no incluyen bases
de datos y el instalador excluye los archivos SQLite al copiar.

Si falla el reemplazo de archivos, el actualizador muestra un error y conserva
los detalles en `error.log` dentro de la carpeta temporal `VeterinariaUpdate-*`.
La instalación y el reinicio mediante PowerShell deben verificarse en Windows.

### Comprobar una actualización en Windows

1. Instalar una primera release y guardar algunos datos de prueba.
2. Publicar una segunda versión mediante un push o merge a `main`, o ejecutar
   **Run workflow** desde GitHub Actions. Esperar a que la release esté disponible.
3. Abrir la versión anterior y comprobar que aparece el aviso.
4. Seleccionar **Actualizar** y verificar el cierre, reemplazo y reinicio.
5. Comprobar que los datos guardados siguen disponibles.

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
