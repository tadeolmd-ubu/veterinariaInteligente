# CORRECCIONES

Documento único de hallazgos y plan de corrección, repartido entre los 3 roles del proyecto.

| Rol | Alcance en el proyecto |
| --- | --- |
| **Front** | `Views/*.axaml` (y `.axaml.cs`), `Styles/*`, `MainWindow.axaml`, `App.axaml`. Maquetación, estilos, diálogos, `Enabled`/`IsVisible` visuales y bindings XAML. |
| **ViewModel** | `ViewModels/*.cs`. Estado de pantalla, comandos, `CanExecute`, limpiar/autorellenar formularios, orquestar servicios y errores. |
| **Backend** | `Service/*.cs`, `Models/*`, `Data/AppDbContext.cs`, `Migrations/*`. Validaciones, consultas EF Core, reglas de negocio y persistencia. |

> Los hallazgos compartidos por más de un rol se resuelven en conjunto: el backend expone el dato/regla, el ViewModel lo maneja y el Front lo muestra.

---

## Tabla resumen

| ID | Sección | Hallazgo | Front | ViewModel | Backend |
| -- | ------- | -------- | :---: | :-------: | :-----: |
| 1 | General | MessageBox en operaciones en vez del texto inferior | ✅ | ✅ | — |
| 2 | General | Listas de Citas/Dueños/Pendientes sin scroll (se salen de pantalla) | ✅ | — | — |
| 3 | Dueños | Aviso de Nuevo Propietario debajo; se pide ventana/sonido | ✅ | ✅ | — |
| 4 | Dueños | Los datos del propietario se autorrellenan al instante | — | ✅ | — |
| 5 | Dueños | Nueva mascota: elegir dueño en ventana + nombre no editable + fecha bug | ✅ | ✅ | — |
| 6 | Dueños | Mal flujo archivar/desarchivar (no se ven ni restauran mascotas archivadas) | ✅ | ✅ | ✅ |
| 7 | Dueños | Sugerencia: archivados en mini-sección gris debajo de los activos | ✅ | ✅ | — |
| 8 | Consultas | Limpiar campos al guardar + no sale el historial de la consulta | ✅ | ✅ | ✅ |
| 9 | Cita | Los campos no se limpian al terminar de crear | — | ✅ | — |
| 10 | Cita | Agenda: fecha bugeada y controles solapados | ✅ | ✅ | — |
| 11 | Cita | Canceladas: no se puede aplicar estado y "Crear" duplica la cita | ✅ | ✅ | ✅ |
| 12 | Cita | Botón "Nueva cita" duplicado | ✅ | — | — |
| 13 | Pendientes | Botón "Nuevo seguimiento" duplicado | ✅ | — | — |
| 14 | Pendientes | "Origen" y "Consulta de realización" son el mismo campo → dejar solo Origen | ✅ | ✅ | — |
| 15 | Vacuna | Todos los botones siempre activos (falta estado por selección) | — | ✅ | — |
| 16 | Vacuna | "Aplicar vacuna" y "Nueva" hacen lo mismo; ubicación confusa | ✅ | ✅ | — |
| 17 | Configuración | No se limpian los campos al crear profesional | — | ✅ | — |
| 18 | Configuración | Faltan nombre, teléfono y dirección de la veterinaria | ✅ | ✅ | ✅ |
| 19 | Configuración | Selector gráfico de carpeta para el respaldo | ✅ | ✅ | — |
| 20 | Configuración | Mover "Generar PDF" a su apartado correspondiente | ✅ | ✅ | — |

---

## Orden sugerido de trabajo

1. **Backend** entrega datos/reglas que hoy bloquean la UI (#6, #8, #11, #18).
2. **ViewModel** agrega comandos, `CanExecute`, estados y limpieza de formularios (#4, #5, #6, #8, #9, #11, #14, #15, #16, #17, #18, #19, #20).
3. **Front** maqueta diálogos, scroll, mini-secciones y reubica botones/PDF (#1, #2, #3, #5, #6, #7, #10, #11, #12, #13, #14, #16, #18, #19, #20).

Recomendación: el **ViewModel** expone primero el contrato (propiedades/comandos/eventos) y el **Front** lo consume.

---

# FRONT — Hallazgos y qué hacer

## #1 Diálogo de mensajes (MessageBox) — General
**Hallazgo:** hoy el resultado se muestra con `<TextBlock Text="{Binding MensajeOperacion}" Classes="caption"/>` en todas las vistas (p. ej. `PropietariosView.axaml:95,130`, `CitasView.axaml:206`). No existe ningún diálogo/mensajería (solo `ActualizacionWindow`).

**Qué hacer:**
- Crear una ventana modal reutilizable (estilo del proyecto, p. ej. `Views/MensajeWindow.axaml`) con título, mensaje, ícono y botón Aceptar.
- Conectarla a la abstracción que dispare el ViewModel y registrarla en `App.axaml.cs` (junto a los servicios).
- (Opcional) reproducir un sonido del sistema en avisos de error.

## #2 Scroll en las listas — General
**Hallazgo:** las listas de resultados de Citas, Dueños y Pendientes se salen de la pantalla. `CitasView.axaml:106`, `PropietariosView.axaml:46` y `PendientesView.axaml:54` colocan el `ListBox` en una fila `*` que no queda bien acotada; la lista de mascotas usa un `WrapPanel` (`PropietariosView.axaml:188-191`) que tampoco scrollea.

**Qué hacer:**
- En las tres vistas, acotar el alto del contenedor de resultados y garantizar `VerticalScrollBarVisibility="Auto"`.
- Revisar los `WrapPanel` de mascotas.
- Probar en resolución mínima (`1100x700`).

## #3 Aviso de Nuevo Propietario — Dueños
**Hallazgo:** el error sale debajo de la sección (`PropietariosView.axaml:95`); se pide ventana emergente y/o sonido.

**Qué hacer:**
- Quitar el `TextBlock` de mensaje bajo la sección si el requisito es que todo salga por diálogo.
- Consumir el diálogo del punto #1.

## #5 Nueva mascota — Dueños
**Hallazgo:** conviene elegir el dueño en una ventana; el nombre del dueño debería verse como campo no editable. La fecha está bugueada (`PropietariosView.axaml:177`).

**Qué hacer:**
- Crear la ventana de selección de propietario.
- En el formulario de mascota, mostrar un campo de solo lectura con el nombre del dueño.
- Corregir el `DatePicker` de fecha de nacimiento según el arreglo del ViewModel.

## #6 Restaurar mascota — Dueños
**Hallazgo:** falta el botón para desarchivar mascotas (hoy solo hay "Archivar seleccionada" y "Restaurar seleccionado", `PropietariosView.axaml:28,140`).

**Qué hacer:**
- Agregar botón "Restaurar mascota" (bind a `RestaurarMascotaCommand`) disponible para mascotas archivadas.

## #7 Mini-sección de archivados — Dueños
**Hallazgo:** al marcar "Ver archivados" (`PropietariosView.axaml:27`) desaparecen los activos.

**Qué hacer:**
- Rediseñar el panel: activos arriba y, debajo, archivados con estilo gris/suave (dueños y mascotas).
- El checkbox solo puede quedar como filtro adicional, nunca debe ocultar a los activos.

## #8 Historial de la consulta — Consultas
**Hallazgo:** el bloque existe (`ConsultasView.axaml:161-194`) pero no se refresca/puebla.

**Qué hacer:**
- Asegurar que el `ItemsControl` (`ConsultasView.axaml:164`) se refresque al cambiar `SelectedConsulta` y darle scroll si crece.

## #10 Agenda de Citas — Cita
**Hallazgo:** en `CitasView.axaml:47-52` los dos `DatePicker` + botón están en una fila `ColumnDefinitions="*,*,Auto"` dentro de 390 px → se solapan.

**Qué hacer:**
- Reacomodar los controles del rango (apilados o con ancho suficiente) para que no se solapen.

## #11 Citas canceladas — Cita
**Hallazgo:** en modo canceladas los botones quedan deshabilitados y el usuario termina usando "Crear" (`CitasView.axaml:155`).

**Qué hacer:**
- Separar visualmente las acciones de una cita cancelada (reactivar/cambiar estado) de las de alta.
- Deshabilitar "Crear" cuando corresponda.

## #12 y #13 Botones duplicados — Cita / Pendientes
**Hallazgo:** "Nueva cita" aparece dos veces (`CitasView.axaml:31` y `:154`); "Nuevo seguimiento" dos veces (`PendientesView.axaml:29` y `:102`).

**Qué hacer:**
- Eliminar la duplicación en ambas vistas.

## #14 Quitar "Consulta de realización" — Pendientes
**Hallazgo:** "Origen" y "Consulta de realización" son percibidos como el mismo campo.

**Qué hacer:**
- Eliminar el bloque de `PendientesView.axaml:141-148`; dejar solo "Origen".

## #16 Aplicar vacuna / Nueva — Vacuna
**Hallazgo:** el botón superior "Aplicar vacuna" (`VacunasView.axaml:109-110`) ejecuta `NuevaAplicacionCommand` (limpiar), lo que confunde.

**Qué hacer:**
- Renombrar/reubicar ese botón para que no se confunda con la acción de guardar; alinearlo con `AgregarAplicacionCommand`.

## #18 Datos de la veterinaria — Configuración
**Hallazgo:** faltan los campos Nombre, Teléfono y Dirección de la veterinaria.

**Qué hacer:**
- Agregar los campos en `ConfiguracionView.axaml` con sus bindings.

## #19 Selector de carpeta — Configuración
**Hallazgo:** hoy es un `TextBox` libre (`ConfiguracionView.axaml:75`).

**Qué hacer:**
- Agregar botón "Elegir carpeta…" que abra `StorageProvider.OpenFolderPickerAsync` y asigne `CarpetaRespaldo`.
- Mantener el `TextBox` como fallback o reemplazarlo.

## #20 Generar PDF en su apartado — Configuración
**Hallazgo:** la generación de PDF está dentro de Configuración (`ConfiguracionView.axaml:82-100`).

**Qué hacer:**
- Quitar ese bloque.
- Agregar acciones de PDF donde corresponde:
  - Consulta → `ConsultasView.axaml` (consulta seleccionada).
  - Expediente → `PropietariosView.axaml`/`ExpedienteView.axaml` (mascota seleccionada).
  - Cartilla → `VacunasView.axaml` (mascota seleccionada).
- Reutilizar el flujo `PdfGenerado`/`NombreArchivoPdf` para guardar el archivo desde la vista.

---

# VIEWMODEL — Hallazgos y qué hacer

## #1 Mensajería (MessageBox) — General
**Hallazgo:** todo termina en `MensajeOperacion` (string) en cada VM: `PropietariosViewModel.cs:121,146,161`, `CitasViewModel.cs:146,179,197`, `ConsultasViewModel.cs`, `VacunasViewModel.cs`, `PendientesViewModel.cs`, `ConfiguracionViewModel.cs`.

**Qué hacer:**
- Definir una abstracción, p. ej. `IDialogService` con `MostrarAsync(titulo, mensaje, tipo)` (o un evento de notificación).
- Reemplazar los `MensajeOperacion = resultado.Mensaje;` por la llamada al diálogo cuando la operación termina.
- Mantener `MensajeOperacion` solo como resumen opcional.

## #3 Aviso en Nuevo Propietario — Dueños
**Hallazgo:** `AgregarPropietarioInternoAsync` (`PropietariosViewModel.cs:110-128`) asigna `MensajeOperacion`.

**Qué hacer:**
- Disparar el diálogo con el motivo del error.

## #4 No autorrellenar datos del propietario — Dueños
**Hallazgo:** `FiltrarPropietarios` selecciona automáticamente el primero (`PropietariosViewModel.cs:287-289`) y los campos están bindeados directo a `SelectedPropietario.*` (`PropietariosView.axaml:112,117,122,127`).

**Qué hacer:**
- No autoseleccionar en `FiltrarPropietarios`.
- Exponer propiedades de edición independientes (`EditarNombre`, `EditarTelefono`, `EditarCorreo`, `EditarDireccion`) que solo se carguen con selección explícita; formulario en blanco hasta que el usuario elija.
- `ActualizarPropietarioInternoAsync` debe guardar desde esas propiedades, no desde `SelectedPropietario.*`.

## #5 Nueva mascota — Dueños
**Hallazgo:** `AgregarMascotaInternoAsync` usa `SelectedPropietario` (`PropietariosViewModel.cs:185-212`); falta estado del dueño elegido en la ventana.

**Qué hacer:**
- Agregar `DuenioSeleccionadoParaMascota` (o `PropietarioSeleccionado`) y usarlo en el alta.
- Revisar `NuevaMascotaFechaNacimiento` (`DateTimeOffset?` → `DateOnly?`): no guardar `MinValue` y pasar el valor correcto al `DatePicker`.

## #6 Archivar / desarchivar mascotas — Dueños
**Hallazgo:** `RecargarAsync` solo carga mascotas activas (`PropietariosViewModel.cs:245`). `OnSelectedPropietarioChanged` filtra `_mascotasCargadas` (solo activas) → las archivadas nunca aparecen (`PropietariosViewModel.cs:86-100`). No hay comando de restaurar mascota. `RestaurarPropietarioInternoAsync` recarga con `MostrandoArchivados = true`, por lo que el dueño restaurado desaparece (`PropietariosViewModel.cs:170-180, 266-290`).

**Qué hacer:**
- Cargar también mascotas archivadas (`ObtenerArchivadosAsync`).
- Mostrar activas + archivadas del dueño (o colecciones separadas).
- Agregar `RestaurarMascotaCommand` que llame a `_mascotaService.RestaurarAsync(SelectedMascota.Id)` y recargue.
- Al restaurar un dueño, actualizar la vista/lista para que quede visible y seleccionado (idealmente pasar a activos).

## #7 Mini-sección de archivados — Dueños
**Hallazgo:** el modo archivados oculta los activos.

**Qué hacer:**
- Exponer colecciones separadas: `PropietariosActivos`, `PropietariosArchivados`, `MascotasActivas`, `MascotasArchivadas`.

## #8 Consultas: limpiar y mostrar historial — Consultas
**Hallazgo:** `AgregarConsultaAsync` limpia y luego `RecargarAsync(resultado.Valor.Id)` (`ConsultasViewModel.cs:172-173`); `FiltrarConsultas` reselecciona y `OnSelectedConsultaChanged` rellena (`ConsultasViewModel.cs:103-115`). El historial solo se arma para activas (`ConsultasViewModel.cs:288-292`).

**Qué hacer:**
- Al guardar, no volver a seleccionar la consulta creada; dejar la selección vacía.
- Poblar los campos solo con selección explícita.
- Cargar `Versiones` también para archivadas (método nuevo del backend) y refrescar el historial al actualizar.

## #9 Cita: no se limpian los campos — Cita
**Hallazgo:** `AgregarCitaAsync` limpia y luego `RecargarAsync(resultado.Valor.Id)` (`CitasViewModel.cs:153-154`); `FiltrarCitas` reselecciona y `OnSelectedCitaChanged` rellena (`CitasViewModel.cs:107-118, 351`).

**Qué hacer:**
- No autoseleccionar la cita recién creada o limpiar después de la recarga.
- Alternativa consistente con #4: separar formulario de alta y de edición.

## #10 Agenda — Cita
**Hallazgo:** rango de agenda (`CitasViewModel.cs:244-278, 318-329`).

**Qué hacer:**
- Revisar `CargarAgendaAsync`/`RecargarAgendaAsync`: validar rangos, normalizar `LocalDateTime`/`Date` y no depender de valores parciales.
- Coordinar con Front el acomodo de los `DatePicker`.

## #11 Citas canceladas — Cita
**Hallazgo:** `PuedeModificar`/`PuedeCancelar` desactivan todo en modo canceladas (`CitasViewModel.cs:101-102`). `AgregarCitaAsync` reutiliza datos precargados por `OnSelectedCitaChanged` → "Crear" duplica.

**Qué hacer:**
- Agregar `ReactivarCitaCommand` (o `CambiarEstadoCitaCanceladaCommand`) usando la nueva operación del backend, habilitado en modo canceladas con `SelectedCita`.
- Evitar que "Crear" reuse datos auto-cargados: deshabilitarlo con `SelectedCita != null` o forzar "Nueva" antes.
- Ajustar `PuedeModificar`/`PuedeCancelar` con habilitación propia para canceladas.

## #14 Pendientes: dejar solo Origen
**Hallazgo:** existen `SelectedConsultaOrigen` y `SelectedConsultaRealizacion` (`PendientesViewModel.cs:43-44`); el segundo se usa en `MarcarRealizadoAsync` (`PendientesViewModel.cs:143`).

**Qué hacer:**
- Eliminar `SelectedConsultaRealizacion` y su lógica.
- `MarcarRealizadoAsync` debe llamar a `MarcarRealizadoAsync(id, fecha, null)` (o reutilizar el origen si el negocio lo pide).
- Limpiar referencias en `OnSelectedMascotaChanged`, `OnSelectedSeguimientoChanged` y `NuevoSeguimiento`.

## #15 Botones de Vacuna
**Hallazgo:** no hay `CanExecute`; `ActualizarVacunaAsync`, `DesactivarVacunaAsync`, `ActivarVacunaAsync` están siempre disponibles (`VacunasViewModel.cs:85-96`).

**Qué hacer:**
- `ActualizarVacunaCommand` y `DesactivarVacunaCommand` → habilitados solo con vacuna seleccionada y `Activo == 1`.
- `ActivarVacunaCommand` → habilitado solo con vacuna seleccionada y `Activo == 0`.
- `AgregarVacunaCommand` (Crear) → habilitado solo sin vacuna seleccionada.
- Agregar `[NotifyCanExecuteChangedFor(...)]` en `SelectedVacuna` e `IsBusy`.

## #16 Aplicar vacuna / Nueva — Vacuna
**Hallazgo:** `NuevaAplicacion` solo limpia (`VacunasViewModel.cs:162-169`) pero está atado al botón "Aplicar vacuna".

**Qué hacer:**
- Dejar `NuevaAplicacionCommand` como "Nueva/limpiar" y que `AgregarAplicacionCommand` sea el único que aplique.
- Coordinar con Front el nombre/posición de los botones.

## #17 Limpiar campos al crear profesional — Configuración
**Hallazgo:** `AgregarProfesionalAsync` hace `RecargarAsync(resultado.Valor?.Id)` y `OnSelectedProfesionalChanged` rellena; nunca llama a `NuevoProfesional` (`ConfiguracionViewModel.cs:75-85, 58-62`).

**Qué hacer:**
- Tras crear, limpiar `Nombre`/`Cedula` y no reseleccionar el profesional creado (reutilizar `NuevoProfesional()`).

## #18 Datos de la veterinaria — Configuración
**Hallazgo:** no hay propiedades ni carga/guardado de `ConfiguracionCliente`.

**Qué hacer:**
- Inyectar `ConfiguracionClienteService`.
- Agregar propiedades (Nombre, Teléfono, Dirección) y comandos `CargarConfiguracionAsync`/`GuardarConfiguracionAsync`.
- Reportar errores por el diálogo (#1).

## #19 Selector de carpeta — Configuración
**Hallazgo:** la carpeta de respaldo se escribe a mano.

**Qué hacer:**
- Agregar comando (p. ej. `SeleccionarCarpetaRespaldoCommand`) que solicite la carpeta al Front y actualice `CarpetaRespaldo`.
- Mantener la edición manual como fallback.

## #20 Mover generación de PDF — Configuración
**Hallazgo:** `GenerarConsultaPdfAsync`/`GenerarExpedientePdfAsync`/`GenerarCartillaPdfAsync` viven en `ConfiguracionViewModel.cs:127-151`.

**Qué hacer:**
- Mover `GenerarConsultaPdfAsync` a `ConsultasViewModel`.
- Mover `GenerarExpedientePdfAsync`/`GenerarCartillaPdfAsync` a `PropietariosViewModel`/`ExpedienteViewModel` y `VacunasViewModel`.
- Conservar el patrón `PdfGenerado`/`NombreArchivoPdf` y que la vista guarde los bytes.

---

# BACKEND — Hallazgos y qué hacer

> **Estado (actualizado 2026-10-13):** los 4 hallazgos de backend están
> implementados, con pruebas, y mergeados a `dev`. No hubo cambios de modelo ni
> migraciones nuevas. Los ViewModels ya pueden consumir los contratos indicados
> en cada hallazgo.

## #6 Archivar / desarchivar mascotas — Dueños
**Hallazgo:** `MascotaService` ya tiene `ObtenerArchivadosAsync` y `RestaurarAsync` (`MascotaService.cs:28-35, 140-152`), pero no expone una consulta por dueño que incluya archivadas ni trae la navegación `Duenio`. `ObtenerPorIdAsync` filtra `FechaArchivado == null` (`MascotaService.cs:37-42`).

**Qué hacer:**
- Agregar un método para obtener mascotas archivadas de un dueño incluyendo la navegación `Duenio` (p. ej. `ObtenerArchivadosPorDuenioAsync(int idDuenio)` o un `ObtenerArchivadosAsync()` con `Include(m => m.Duenio)`).
- Verificar que `RestaurarAsync` funcione aunque el dueño siga archivado.
- Si aplica, agregar un método de lectura sin el filtro de `FechaArchivado`.

**Estado:** ✅ Implementado (PR #26). `MascotaService.ObtenerArchivadasDeDueniosActivosAsync()`
devuelve las mascotas archivadas cuyo dueño está activo, con la navegación `Duenio`
cargada; las de un dueño archivado quedan ocultas hasta restaurar al dueño.
`RestaurarAsync` ya funcionaba con el dueño archivado (cubierto por test).

## #8 Historial de consultas archivadas — Consultas
**Hallazgo:** las versiones se persisten en `ConsultaService.RegistrarVersionSiCambio` (`ConsultaService.cs:224-243`), pero `ObtenerPorIdAsync` excluye archivadas (`ConsultaService.cs:41-49`).

**Qué hacer:**
- Exponer `ObtenerVersionesAsync(int idConsulta)` o permitir obtener el detalle sin filtrar por estado, para que el historial no quede vacío en archivadas.

**Estado:** ✅ Implementado (PR #27). `ConsultaService.ObtenerVersionesAsync(int idConsulta)`
devuelve el historial sin filtrar por estado. Además se agregó
`.ThenInclude(v => v.Profesional)` en `ObtenerPorIdAsync` y `ObtenerExpedienteAsync`
para que el nombre del profesional llegue a la vista también en consultas activas.

## #11 Cambiar estado de una cita cancelada — Cita
**Hallazgo:** `CitaService.CambiarEstadoAsync` rechaza citas canceladas (`CitaService.cs:155-176`).

**Qué hacer:**
- Agregar una operación explícita, p. ej. `ReactivarAsync(int id, EstadoCita nuevoEstado)`, que permita partir de `Cancelada`.
- Definir la regla de negocio (¿desde Cancelada se puede volver a `Pendiente`, `Confirmada`, `Atendida`?) y validar fecha si corresponde.
- Mantener `CambiarEstadoAsync` para no canceladas o unificar con validación clara.

**Estado:** ✅ Implementado (PR #28). `CitaService.ReactivarAsync(int id, EstadoCita nuevoEstado)`
parte solo de una cita `Cancelada`, acepta cualquier destino menos `Cancelada` y exige
fecha futura solo cuando el destino es `Pendiente`/`Confirmada`. `CambiarEstadoAsync`
se mantiene igual.

## #18 Configuración de la veterinaria — Configuración
**Hallazgo:** `ConfiguracionCliente` existe en el modelo y en `AppDbContext` (`AppDbContext.cs:17, 226-240`), pero no hay servicio que lo lea/guarde.

**Qué hacer:**
- Crear `Service/ConfiguracionClienteService.cs` con:
  - `ObtenerAsync()` → registro único (o `null` si no existe).
  - `GuardarAsync(nombre, telefono, direccion, idProfesionalDefault?)` → crea/actualiza el registro único.
  - Validaciones equivalentes a `DuenioService` (nombre/teléfono obligatorios, formato de teléfono, longitudes según `AppDbContext`).
- Decidir el uso de `IdProfesionalDefault` y dejar el contrato claro para el ViewModel.
- Crear migración/semilla solo si hace falta un registro por defecto.

**Estado:** ✅ Implementado (PR #29). Nuevo `Service/ConfiguracionClienteService.cs`:
`ObtenerAsync()` devuelve el registro único (o `null`) con `ProfesionalDefault` cargado,
y `GuardarAsync(nombre, telefono, direccion, idProfesionalDefault?)` crea el registro la
primera vez y lo actualiza después, validando que el profesional default sea activo si
viene. Sin migración ni semilla: el registro se crea en el primer guardado. Se acordó
mantener `IdProfesionalDefault` como parámetro opcional para el ViewModel.

## Transversales (sin cambios)
- **#19:** `RespaldoService.CrearRespaldoAsync` ya recibe la carpeta (`RespaldoService.cs:13-14`).
- **#20:** `PdfService` ya expone `GenerarConsultaAsync`, `GenerarExpedienteAsync` y `GenerarCartillaVacunacionAsync` (`PdfService.cs:17,43,93`).
- **#1 (opcional):** si se centraliza la mensajería como servicio, definir aquí la interfaz que implementará el Front.
