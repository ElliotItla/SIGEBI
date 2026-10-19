# SIGEBI: Sistema de Gestión de Bibliotecas

Proyecto final de Programación II, ITLA.

* **Estudiante:** Elliot Adalberto Eusebio Hernández (matrícula 2025-1155)
* **Profesor:** Francis Ramírez
* **Requisitos:** `docs/SRS_SIGEBI_v1_0.pdf`

## Arquitectura

Arquitectura en capas, con C# y .NET 8.

| Proyecto | Capa | Contenido |
|---|---|---|
| `SIGEBI.Web` | Presentación | ASP.NET Core MVC con HTML y CSS: controladores, vistas y estilos |
| `SIGEBI.BL` | Negocio | `Services` con las reglas de cada módulo y `DependencyInjection.cs` |
| `SIGEBI.DAL` | Datos | `Data` (DbContext), `Models` (entidades, una subcarpeta por schema) y `Repositories` |

Flujo de una petición: navegador → `Web` → `BL` → `DAL` → SQL Server.

`Web` también referencia a `DAL`, pero solo para registrar el acceso a datos al arrancar.

### Entidades por schema (`SIGEBI.DAL/Models`)

| Schema | Entidades |
|---|---|
| Seguridad | `Usuario` |
| Catalogo | `Recurso`, `Ejemplar`, `HistorialEstadoEjemplar` |
| Prestamos | `Prestamo`, `Penalizacion` |
| Notificaciones | `Notificacion` |
| Auditoria | `RegistroAuditoria` |
| Configuracion | `PoliticaPrestamo`, `ParametroSistema` |

## Base de datos

SQL Server, creada con un script y sin migraciones.

1. Abrir `docs/SIGEBI_BaseDeDatos.sql` en SQL Server Management Studio y ejecutarlo completo, una sola vez.
2. Ajustar la cadena de conexión `Sigebi` en `SIGEBI.Web/appsettings.json` según tu servidor.

## Cómo ejecutar

1. Abrir `SIGEBI.sln` en Visual Studio (restaura los paquetes de NuGet solo).
2. Establecer `SIGEBI.Web` como proyecto de inicio y ejecutar.

## Módulos y ramas

Cada módulo se desarrolla en su propia rama a partir de `main`.

| Módulo | Rama |
|---|---|
| Gestión de usuarios | `feature/usuarios` |
| Catálogo bibliográfico | `feature/catalogo` |
| Gestión de préstamos | `feature/prestamos` |
| Devoluciones | `feature/devoluciones` |
| Penalizaciones | `feature/penalizaciones` |
| Notificaciones | `feature/notificaciones` |
| Reportes y auditoría | `feature/reportes-auditoria` |
