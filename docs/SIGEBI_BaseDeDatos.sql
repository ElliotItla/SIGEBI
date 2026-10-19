-- =====================================================================
-- SIGEBI - Script de base de datos (SQL Server)
-- Ejecutar completo, una sola vez, sobre un servidor sin base SIGEBI.
-- Cada schema corresponde a una subcarpeta de SIGEBI.Domain/Entities.
-- =====================================================================

IF DB_ID('SIGEBI') IS NULL
    CREATE DATABASE SIGEBI;
GO

USE SIGEBI;
GO

CREATE SCHEMA Seguridad;
GO
CREATE SCHEMA Catalogo;
GO
CREATE SCHEMA Prestamos;
GO
CREATE SCHEMA Notificaciones;
GO
CREATE SCHEMA Auditoria;
GO
CREATE SCHEMA Configuracion;
GO

-- Las tablas con las columnas de AuditEntity (Id, FechaCreacion, UsuarioCreacion,
-- FechaModificacion, UsuarioModificacion, Estatus) son: Usuario, Recurso, Ejemplar,
-- Prestamo, Penalizacion, PoliticaPrestamo y ParametroSistema.
-- Los tres registros que solo se agregan (Historial, Notificacion y RegistroAuditoria)
-- llevan su propia fecha y no se modifican.

-- ---------------------------------------------------------------------
-- SEGURIDAD
-- Estatus = 1 es usuario activo. Desactivar es poner Estatus en 0 (no se borra).
-- ---------------------------------------------------------------------
CREATE TABLE Seguridad.Usuario (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Identificador       NVARCHAR(30)  NOT NULL UNIQUE,   -- matricula, cedula o codigo
    Nombre              NVARCHAR(80)  NOT NULL,
    Apellido            NVARCHAR(80)  NOT NULL,
    Correo              NVARCHAR(120) NOT NULL UNIQUE,
    ContrasenaHash      NVARCHAR(200) NOT NULL,          -- nunca la clave en texto
    Tipo                NVARCHAR(25)  NOT NULL
        CHECK (Tipo IN ('Estudiante','Docente','PersonalBibliotecario','Administrador','Auditor')),
    IntentosFallidos    INT           NOT NULL DEFAULT 0,
    BloqueadoHasta      DATETIME2     NULL,
    FechaCreacion       DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT           NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2     NULL,
    UsuarioModificacion INT           NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT           NOT NULL DEFAULT 1
);
GO

-- ---------------------------------------------------------------------
-- CATALOGO
-- ---------------------------------------------------------------------
CREATE TABLE Catalogo.Recurso (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Codigo              NVARCHAR(30)  NOT NULL UNIQUE,   -- ISBN u otro identificador unico
    Titulo              NVARCHAR(200) NOT NULL,
    Autor               NVARCHAR(150) NOT NULL,
    Categoria           NVARCHAR(80)  NOT NULL,
    FechaCreacion       DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT           NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2     NULL,
    UsuarioModificacion INT           NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT           NOT NULL DEFAULT 1   -- 0 = dado de baja
);
GO

-- El estado Reservado se conserva, pero todavia no tiene flujo en el sistema.
CREATE TABLE Catalogo.Ejemplar (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    RecursoId           INT          NOT NULL REFERENCES Catalogo.Recurso(Id),
    CodigoEjemplar      NVARCHAR(40) NOT NULL UNIQUE,
    Estado              NVARCHAR(20) NOT NULL DEFAULT 'Disponible'
        CHECK (Estado IN ('Disponible','Prestado','Reservado','FueraDeServicio')),
    FechaCreacion       DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT          NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2    NULL,
    UsuarioModificacion INT          NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT          NOT NULL DEFAULT 1
);
GO

CREATE TABLE Catalogo.HistorialEstadoEjemplar (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    EjemplarId     INT           NOT NULL REFERENCES Catalogo.Ejemplar(Id),
    EstadoAnterior NVARCHAR(20)  NULL,
    EstadoNuevo    NVARCHAR(20)  NOT NULL,
    Fecha          DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UsuarioId      INT           NULL REFERENCES Seguridad.Usuario(Id),   -- NULL si lo hizo el sistema
    Motivo         NVARCHAR(200) NULL
);
GO

-- ---------------------------------------------------------------------
-- PRESTAMOS
-- FechaCreacion es la fecha de la solicitud.
-- El ejemplar se elige al solicitar y se marca Prestado al aprobar.
-- Un prestamo esta vencido cuando Estado = 'Activo' y FechaLimite ya paso (se calcula, no se guarda).
-- Las solicitudes rechazadas por el sistema (penalizacion, limite, no disponible)
-- se registran en Auditoria.RegistroAuditoria con su motivo.
-- Estado 'Rechazado' es cuando el personal rechaza una solicitud pendiente.
-- ---------------------------------------------------------------------
CREATE TABLE Prestamos.Prestamo (
    Id                    INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId             INT           NOT NULL REFERENCES Seguridad.Usuario(Id),
    EjemplarId            INT           NOT NULL REFERENCES Catalogo.Ejemplar(Id),
    Estado                NVARCHAR(15)  NOT NULL DEFAULT 'Pendiente'
        CHECK (Estado IN ('Pendiente','Rechazado','Activo','Devuelto')),
    FechaInicio           DATETIME2     NULL,
    FechaLimite           DATETIME2     NULL,
    FechaDevolucion       DATETIME2     NULL,
    MotivoRechazo         NVARCHAR(200) NULL,
    AprobadoPorId         INT           NULL REFERENCES Seguridad.Usuario(Id),
    RecibidoPorId         INT           NULL REFERENCES Seguridad.Usuario(Id),
    ObservacionDevolucion NVARCHAR(300) NULL,
    FechaCreacion         DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion       INT           NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion     DATETIME2     NULL,
    UsuarioModificacion   INT           NULL REFERENCES Seguridad.Usuario(Id),
    Estatus               BIT           NOT NULL DEFAULT 1
);
GO

-- Un mismo ejemplar no puede tener dos prestamos activos a la vez
CREATE UNIQUE INDEX UX_Prestamo_EjemplarActivo
    ON Prestamos.Prestamo(EjemplarId) WHERE Estado = 'Activo';
GO

-- Un solo tipo de penalizacion (retraso). Queda Activa hasta que el personal la resuelve.
-- FechaCreacion es la fecha en que se genero.
CREATE TABLE Prestamos.Penalizacion (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId           INT           NOT NULL REFERENCES Seguridad.Usuario(Id),
    PrestamoId          INT           NOT NULL REFERENCES Prestamos.Prestamo(Id),
    DiasRetraso         INT           NOT NULL CHECK (DiasRetraso > 0),
    Descripcion         NVARCHAR(200) NOT NULL,
    Estado              NVARCHAR(10)  NOT NULL DEFAULT 'Activa'
        CHECK (Estado IN ('Activa','Resuelta')),
    FechaResolucion     DATETIME2     NULL,
    ResueltaPorId       INT           NULL REFERENCES Seguridad.Usuario(Id),
    MotivoResolucion    NVARCHAR(200) NULL,    -- pago, cumplimiento de condicion o exoneracion autorizada
    FechaCreacion       DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT           NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2     NULL,
    UsuarioModificacion INT           NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT           NOT NULL DEFAULT 1
);
GO

-- Un prestamo genera como maximo una penalizacion
CREATE UNIQUE INDEX UX_Penalizacion_Prestamo
    ON Prestamos.Penalizacion(PrestamoId);
GO

-- ---------------------------------------------------------------------
-- NOTIFICACIONES (solo dentro del sistema, sin correo)
-- ---------------------------------------------------------------------
CREATE TABLE Notificaciones.Notificacion (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId  INT           NOT NULL REFERENCES Seguridad.Usuario(Id),
    PrestamoId INT           NULL REFERENCES Prestamos.Prestamo(Id),
    TipoEvento NVARCHAR(40)  NOT NULL
        CHECK (TipoEvento IN ('ProximoVencimiento','PenalizacionGenerada','PrestamoSolicitado',
                              'PrestamoConfirmado','PrestamoDevuelto','PenalizacionResuelta')),
    Mensaje    NVARCHAR(300) NOT NULL,
    Leida      BIT           NOT NULL DEFAULT 0,
    FechaEnvio DATETIME2     NOT NULL DEFAULT SYSDATETIME()
);
GO

-- No se avisa dos veces el mismo vencimiento de un prestamo
CREATE UNIQUE INDEX UX_Notificacion_VencimientoUnico
    ON Notificaciones.Notificacion(PrestamoId) WHERE TipoEvento = 'ProximoVencimiento';
GO

-- ---------------------------------------------------------------------
-- AUDITORIA (solo se agrega, nunca se modifica ni se borra)
-- ---------------------------------------------------------------------
CREATE TABLE Auditoria.RegistroAuditoria (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId  INT           NULL REFERENCES Seguridad.Usuario(Id),   -- NULL si lo hizo el sistema
    Modulo     NVARCHAR(30)  NOT NULL,
    TipoAccion NVARCHAR(60)  NOT NULL,
    Detalle    NVARCHAR(300) NULL,
    Resultado  NVARCHAR(20)  NOT NULL
        CHECK (Resultado IN ('Exitoso','Rechazado','Error')),
    Fecha      DATETIME2     NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE TRIGGER Auditoria.TR_RegistroAuditoria_SoloLectura
ON Auditoria.RegistroAuditoria
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50001, 'El registro de auditoria no se puede modificar ni eliminar.', 1;
END;
GO

-- ---------------------------------------------------------------------
-- CONFIGURACION (la maneja el administrador)
-- ---------------------------------------------------------------------
CREATE TABLE Configuracion.PoliticaPrestamo (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    TipoUsuario         NVARCHAR(25) NOT NULL,
    MaxPrestamos        INT          NOT NULL,
    DiasPrestamo        INT          NOT NULL,
    FechaCreacion       DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT          NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2    NULL,
    UsuarioModificacion INT          NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT          NOT NULL DEFAULT 1,
    CONSTRAINT UQ_PoliticaPrestamo_Tipo     UNIQUE (TipoUsuario),
    CONSTRAINT CK_PoliticaPrestamo_Tipo     CHECK (TipoUsuario IN ('Estudiante','Docente')),
    CONSTRAINT CK_PoliticaPrestamo_Max      CHECK (MaxPrestamos > 0),
    CONSTRAINT CK_PoliticaPrestamo_Dias     CHECK (DiasPrestamo > 0)
);
GO

CREATE TABLE Configuracion.ParametroSistema (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Clave               NVARCHAR(40) NOT NULL UNIQUE,
    Valor               INT          NOT NULL,
    FechaCreacion       DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    UsuarioCreacion     INT          NULL REFERENCES Seguridad.Usuario(Id),
    FechaModificacion   DATETIME2    NULL,
    UsuarioModificacion INT          NULL REFERENCES Seguridad.Usuario(Id),
    Estatus             BIT          NOT NULL DEFAULT 1
);
GO

-- Valores iniciales de ejemplo: el administrador puede cambiarlos
INSERT INTO Configuracion.PoliticaPrestamo (TipoUsuario, MaxPrestamos, DiasPrestamo) VALUES
    ('Estudiante', 3, 7),
    ('Docente',    5, 15);

INSERT INTO Configuracion.ParametroSistema (Clave, Valor) VALUES
    ('DiasAvisoVencimiento', 2),
    ('MaxIntentosLogin',     5),
    ('MinutosBloqueo',       15);
GO

-- El primer administrador NO se crea aqui: la aplicacion lo crea al arrancar
-- si no existe ningun usuario, con datos tomados de appsettings.json.
