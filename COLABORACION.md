# Colaboración en equipo — veterinariaInteligente

Trabajamos 3 personas en ramas simultáneas sobre un desarrollo por **capas**.

| Persona       | Capa        | Rol principal                |
|---------------|-------------|------------------------------|
| **Tadeo**     | `services`  | Backend: lógica, EF Core     |
| **Carlos**    | `viewmodels`| Estado, comandos, navegación |
| **Ricardo**   | `views`     | Diseño de interfaz (UI)      |

Cada persona es **dueña de su capa**: no toca archivos de otra capa. Así evitamos
conflictos de merge casi por completo.

---

## 1. Capas y archivos que le pertenecen a cada uno

| Capa | Dueño  | Qué hace | Archivos que puede tocar |
|------|--------|----------|--------------------------|
| `services` | Tadeo | Reglas de negocio, CRUD sobre EF Core, validaciones, seeding de catálogos, migraciones si el modelo cambia | `Service/*.cs`, `Data/*.cs`, `Models/*.cs` (solo si hay que ajustar el modelo), `Migrations/*` |
| `viewmodels` | Carlos | Clases de estado, comandos de UI, conexión entre `views` y `services`, navegación (`MainViewModel`), registro de dependencias (DI) | `ViewModels/*.cs`, `App.axaml.cs`, `Program.cs`, `MainWindow.axaml.cs` |
| `views` | Ricardo | Layout, estilos, converters, presentación visual de cada pantalla | `Views/*.axaml` + code-behind mínimo, `Styles/*` |

**Archivos compartidos (con coordinación):**

- `App.axaml`: Ricardo agrega sus `Styles` y Carlos registra dependencias en `App.axaml.cs`. Avísense antes de tocar este archivo.
- `MainWindow.axaml`: Carlos define la navegación (DataContext + contenido); Ricardo puede re-estilizarlo. Lo arma primero Carlos.

**Regla de oro:** si el cambio que vas a hacer está en una carpeta que no es tuya,
NO lo hagas vos: pedíselo al dueño de esa capa.

---

## 2. Módulos del sistema

Trabajamos por módulos. Al empezar un módulo, los tres abandonan sus ramas viejas.

| Código   | Módulo | Detalle |
|----------|--------|--------------------------|
| `m1-inicio` | Inicio | Dashboard con resumen del día |
| `m2-propietarios` | Propietarios y mascotas | `Duenio`, `Mascota`, expediente |
| `m3-consultas` | Consultas | `Consulta`, versiones, expediente |
| `m4-citas` | Citas | `Cita`, agenda |
| `m5-pendientes` | Pendientes / Seguimientos | `Seguimiento` |
| `m6-vacunas` | Vacunas | `Vacuna`, `AplicacionVacuna` |
| `m7-configuracion` | Configuración | Profesionales, respaldo, PDFs |

Se arranca por **`m2-propietarios`** como piloto para fijar los patrones.

---

## 3. Orden de trabajo por módulo (contract-first)

Las capas son dependientes: `views → viewmodels → services`. Para que los 3 trabajen
en paralelo sin bloqueo:

1. **Tadeo** define y mergea rápido el **contrato** del módulo: las firmas públicas del
   `Service` (sin implementación completa o con implementación básica). Merge a `main` lo antes posible.
2. **Carlos** implementa los `ViewModels` contra ese contrato. Puede arrancar apenas se mergea.
3. **Ricardo** implementa las `Views` contra los nombres expuestos por los `ViewModels`.

En paralelo, cada uno puede avanzar en un **módulo distinto** con el mismo esquema,
siempre que su capa tenga el contrato definido en `main`.

**El contrato es sagrado:** si Tadeo cambia una firma de servicio, Carlos y Ricardo
rompen. Antes de cambiar una firma ya mergeada, avisar al grupo y hacerlo en un PR chico.

---

## 4. Nombres de ramas y Pull Requests

Patrón de rama:

```text
feat/<capa>/<codigo-modulo>
```

| Persona  | Rama para `m2-propietarios`        | Título del PR                          |
|----------|------------------------------------|----------------------------------------|
| Tadeo    | `feat/services/m2-propietarios`    | `services: m2-propietarios - CRUD Duenio y Mascota` |
| Carlos   | `feat/viewmodels/m2-propietarios`  | `viewmodels: m2-propietarios - VMs e integración`   |
| Ricardo  | `feat/views/m2-propietarios`       | `views: m2-propietarios - pantallas y estilo`       |

Reglas:

- Una rama = una capa = un módulo. Nada de mezclas.
- Nombrar la rama con el código de módulo siempre (`m2-...`).
- El prefijo del título del PR = la capa (`services:` / `viewmodels:` / `views:`).
- Commits con convención del README: `feat:`, `fix:`, `docs:`, `test:`, `build:`, `chore:`.

Rutina diaria:

```bash
git switch main
git pull origin main
git switch -c feat/<capa>/<modulo>
```

---

## 5. Convenciones de contrato entre capas

Esto es lo que evita las confusiones. Nadie adivina: se usa exactamente esto.

### 5.1 Servicios (los define Tadeo — es el contrato)

- Clase por entidad en `Service/<X>Service.cs`.
- Métodos asíncronos con nombres de dominio en español:

| Método | Uso |
|--------|-----|
| `Task<List<T>> ObtenerTodosAsync()` | Lista completa |
| `Task<T?> ObtenerPorIdAsync(int id)` | Uno por id (si no existe, `null`) |
| `Task<Resultado<T>> AgregarAsync(...)` | Alta con validación |
| `Task<Resultado> ActualizarAsync(...)` | Modificación |
| `Task<Resultado> ArchivarAsync(int id)` | **Eliminación lógica** (`FechaArchivado`). Nunca borrado físico |

- Tipo compartido `Resultado<T>` en `Service/Resultado.cs`:

```csharp
public record Resultado(bool Correcta, string Mensaje);
public record Resultado<T>(bool Correcta, string Mensaje, T? Valor);
```

- Los `Resultado` uso para validaciones de negocio. Siempre devolverlos en operaciones de escritura.

### 5.2 ViewModels (los define Carlos)

- Clase por vista en `ViewModels/<X>ViewModel.cs`.
- `CommunityToolkit.Mvvm`: campo privado `camelCase` con `[ObservableProperty]` → propiedad pública `PascalCase`.
- Comandos: `[RelayCommand]` sobre método `Xxx` → propiedad `XxxCommand`.
- Colecciones: `ObservableCollection<T>` llamado `<x>Items`; selección: `Selected<x>`.
- Toda propiedad que la vista va a enlazar es **pública** y con nombre estable.

### 5.3 Views (los define Ricardo)

- Un `UserControl` por ViewModel: `Views/<X>View.axaml`.
- Siempre enlazar con `{Binding <NombreExactoDelVM>}`. No inventar nombres.
- Estilos y converters en `Styles/`, no sueltos en cada vista.
- Respetar el diseño acordado del equipo (paleta y tipografía definidas por Ricardo).

---

## 6. Antes de abrir un PR

```bash
dotnet build veterinariaInteligente.sln   # debe terminar con 0 errores
dotnet test veterinariaInteligente.sln
git diff --check
git status
```

En la descripción del PR: módulo, capa, contratos usados/exigidos y cualquier aviso al equipo.

## 7. Definición de "hecho" por capa

| Capa | Está hecho cuando... |
|------|---------------------|
| `services` | Compila, valida reglas y devuelve `Resultado<>`; migraciones al día si tocó el modelo. |
| `viewmodels` | Compila, expone las propiedades y comandos que la vista consume; `main` sigue compilando. |
| `views` | La pantalla se ve y se enlaza sin errores de binding; funciona en ejecución. |

El módulo está completo cuando las 3 capas del mismo módulo fueron mergeadas a `main`.

---

## 8. Orden de integración sugerido

Por seguridad, cuando terminen un módulo, integrar siempre en este orden:

1. `feat/services/<modulo>` → merge a `main`.
2. `feat/viewmodels/<modulo>` → merge a `main`.
3. `feat/views/<modulo>` → merge a `main`.

Así `main` nunca queda con una vista que llama a un servicio que no existe.

---

## 9. Setup inicial

Para preparar el entorno ver `README.md` (clonar, restaurar, crear base local, compilar).