/* =============================================================================
   Migración: 2026-06-16
   Cambios:
     1. WhatsAppLog: tabla nueva para registrar los envíos de WhatsApp FALLIDOS.
   Aplicar sobre SQL Server.
   ============================================================================= */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ─── 1. Tabla WhatsAppLog ────────────────────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'WhatsAppLog' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.WhatsAppLog (
        id_log         INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_WhatsAppLog PRIMARY KEY,
        /* Referencia suave a Orden (sin FK dura: las órdenes PENDIENTE se pueden borrar) */
        id_orden       INT            NULL,
        tipo           VARCHAR(20)    NULL,   -- NEGOCIO | CLIENTE
        destinatario   VARCHAR(20)    NULL,
        plantilla      VARCHAR(100)   NULL,
        estado         VARCHAR(20)    NOT NULL
            CONSTRAINT DF_WhatsAppLog_estado DEFAULT 'FALLIDO',
        codigo_http    INT            NULL,
        mensaje_error  NVARCHAR(MAX)  NULL,
        payload_json   NVARCHAR(MAX)  NULL,
        fecha_intento  DATETIME       NOT NULL
            CONSTRAINT DF_WhatsAppLog_fecha_intento DEFAULT GETDATE()
    );

    CREATE INDEX IX_WhatsAppLog_id_orden ON dbo.WhatsAppLog(id_orden);
END
GO

COMMIT TRANSACTION;
GO

PRINT 'Migración 2026-06-16 aplicada correctamente.';
GO
