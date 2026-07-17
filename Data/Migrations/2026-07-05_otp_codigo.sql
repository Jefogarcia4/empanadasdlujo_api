/* =============================================================================
   Migración: 2026-07-05
   Cambios:
     1. OtpCodigo : reto OTP para el portal de clientes (login por WhatsApp).
                    Guarda el HASH del código (no el texto plano), expiración,
                    contador de intentos y bandera de un solo uso.
   Aplicar sobre SQL Server.
   ============================================================================= */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ─── 1. Tabla OtpCodigo ──────────────────────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'OtpCodigo' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.OtpCodigo (
        id_otp         INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_OtpCodigo PRIMARY KEY,
        /* Teléfono normalizado (solo dígitos) al que se envió el código. */
        telefono       VARCHAR(20)    NOT NULL,
        /* Hash SHA-256 del código de 6 dígitos; nunca se almacena en texto plano. */
        codigo_hash    VARCHAR(200)   NOT NULL,
        expira_en      DATETIME       NOT NULL,
        intentos       INT            NOT NULL
            CONSTRAINT DF_OtpCodigo_intentos DEFAULT 0,
        consumido      BIT            NOT NULL
            CONSTRAINT DF_OtpCodigo_consumido DEFAULT 0,
        fecha_creacion DATETIME       NOT NULL
            CONSTRAINT DF_OtpCodigo_fecha_creacion DEFAULT GETDATE()
    );

    /* Búsqueda del código activo por teléfono. */
    CREATE INDEX IX_OtpCodigo_telefono
        ON dbo.OtpCodigo(telefono, consumido, expira_en);
END
GO

COMMIT TRANSACTION;
GO

PRINT 'Migración 2026-07-05 aplicada correctamente.';
GO
