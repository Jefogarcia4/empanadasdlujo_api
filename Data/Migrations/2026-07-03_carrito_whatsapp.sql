/* =============================================================================
   Migración: 2026-07-03
   Cambios:
     1. CarritoWhatsApp        : carrito borrador generado desde WhatsApp
                                 (info del cliente + estado, SIN crear Cliente/Orden).
     2. CarritoWhatsAppDetalle : items del carrito (SKU o combo + cantidad).
   Aplicar sobre SQL Server.
   ============================================================================= */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ─── 1. Tabla CarritoWhatsApp ────────────────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'CarritoWhatsApp' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.CarritoWhatsApp (
        id_carrito        INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CarritoWhatsApp PRIMARY KEY,
        /* Identificador opaco del link. */
        token             UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_CarritoWhatsApp_token DEFAULT NEWID(),
        /* Datos del cliente (aún sin crear el registro Cliente). */
        nombre            NVARCHAR(100)  NULL,
        apellidos         NVARCHAR(100)  NULL,
        telefono          VARCHAR(20)    NULL,
        email             NVARCHAR(100)  NULL,
        direccion         NVARCHAR(200)  NULL,
        casa_apartamento  NVARCHAR(100)  NULL,
        ciudad            NVARCHAR(100)  NULL,
        departamento      NVARCHAR(100)  NULL,
        codigo_postal     VARCHAR(20)    NULL,
        pais              NVARCHAR(50)   NULL,
        observaciones     NVARCHAR(500)  NULL,
        /* Estado / conversión. */
        estado            VARCHAR(20)    NOT NULL
            CONSTRAINT DF_CarritoWhatsApp_estado DEFAULT 'ACTIVO'
            CONSTRAINT chk_carrito_estado CHECK (estado IN ('ACTIVO','CONVERTIDO')),
        /* Referencia suave a la Orden creada al convertir (sin FK dura). */
        id_orden          INT            NULL,
        fecha_creacion    DATETIME       NOT NULL
            CONSTRAINT DF_CarritoWhatsApp_fecha_creacion DEFAULT GETDATE(),
        fecha_conversion  DATETIME       NULL
    );

    CREATE UNIQUE INDEX UX_CarritoWhatsApp_token ON dbo.CarritoWhatsApp(token);
END
GO

/* ─── 2. Tabla CarritoWhatsAppDetalle ─────────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'CarritoWhatsAppDetalle' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.CarritoWhatsAppDetalle (
        id_detalle   INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CarritoWhatsAppDetalle PRIMARY KEY,
        id_carrito   INT           NOT NULL,
        codigo_sku   VARCHAR(20)   NULL,
        id_combo     INT           NULL,
        cantidad     INT           NOT NULL,
        CONSTRAINT FK_CarritoWhatsAppDetalle_Carrito
            FOREIGN KEY (id_carrito) REFERENCES dbo.CarritoWhatsApp(id_carrito) ON DELETE CASCADE,
        CONSTRAINT chk_carrito_cantidad CHECK (cantidad > 0),
        CONSTRAINT chk_carrito_detalle_sku_o_combo CHECK (
            (codigo_sku IS NOT NULL AND id_combo IS NULL) OR
            (codigo_sku IS NULL AND id_combo IS NOT NULL)
        )
    );

    CREATE INDEX IX_CarritoWhatsAppDetalle_id_carrito ON dbo.CarritoWhatsAppDetalle(id_carrito);
END
GO

COMMIT TRANSACTION;
GO

PRINT 'Migración 2026-07-03 aplicada correctamente.';
GO
