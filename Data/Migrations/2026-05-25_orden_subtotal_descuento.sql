/* =============================================================================
   Migración: 2026-05-25
   Cambios:
     1. Cliente:        agregar columna guardar_info
     2. Orden:          quitar id_lista (FK lista_precio) + agregar subtotal y descuento
     3. Orden_Detalle:  agregar precio_paquete_detal y aplica_mayorista
   Aplicar sobre SQL Server.
   ============================================================================= */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ─── 1. Cliente.guardar_info ─────────────────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'guardar_info' AND Object_ID = OBJECT_ID('dbo.Cliente')
)
BEGIN
    ALTER TABLE dbo.Cliente
        ADD guardar_info BIT NOT NULL CONSTRAINT DF_Cliente_guardar_info DEFAULT 0;
END
GO

/* ─── 2. Orden: drop FK + columna id_lista ────────────────────────────────── */
DECLARE @fkName SYSNAME;

SELECT @fkName = fk.name
FROM   sys.foreign_keys fk
JOIN   sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN   sys.columns c
       ON fkc.parent_object_id = c.object_id
      AND fkc.parent_column_id = c.column_id
WHERE  fk.parent_object_id = OBJECT_ID('dbo.Orden')
  AND  c.name = 'id_lista';

IF @fkName IS NOT NULL
    EXEC('ALTER TABLE dbo.Orden DROP CONSTRAINT ' + @fkName);
GO

/* Eliminar index si existe en id_lista antes de drop column */
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    JOIN  sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    JOIN  sys.columns c        ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID('dbo.Orden') AND c.name = 'id_lista'
)
BEGIN
    DECLARE @ixName SYSNAME;
    SELECT TOP 1 @ixName = i.name
    FROM   sys.indexes i
    JOIN   sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    JOIN   sys.columns c        ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE  i.object_id = OBJECT_ID('dbo.Orden') AND c.name = 'id_lista';

    IF @ixName IS NOT NULL
        EXEC('DROP INDEX [' + @ixName + '] ON dbo.Orden');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'id_lista' AND Object_ID = OBJECT_ID('dbo.Orden')
)
BEGIN
    ALTER TABLE dbo.Orden DROP COLUMN id_lista;
END
GO

/* ─── 3. Orden: agregar subtotal y descuento ──────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'subtotal' AND Object_ID = OBJECT_ID('dbo.Orden')
)
BEGIN
    ALTER TABLE dbo.Orden
        ADD subtotal DECIMAL(14,2) NOT NULL
            CONSTRAINT DF_Orden_subtotal DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'descuento' AND Object_ID = OBJECT_ID('dbo.Orden')
)
BEGIN
    ALTER TABLE dbo.Orden
        ADD descuento DECIMAL(14,2) NOT NULL
            CONSTRAINT DF_Orden_descuento DEFAULT 0;
END
GO

/* Backfill: para órdenes históricas, subtotal := total y descuento := 0 */
UPDATE dbo.Orden
SET    subtotal = total
WHERE  subtotal = 0 AND total > 0;
GO

/* ─── 4. Orden_Detalle: drop columna subtotal computada antes de tocar columnas ── */
DECLARE @subDef SYSNAME;
SELECT @subDef = dc.name
FROM   sys.default_constraints dc
JOIN   sys.columns c ON c.default_object_id = dc.object_id
WHERE  c.object_id = OBJECT_ID('dbo.Orden_Detalle') AND c.name = 'subtotal';

IF @subDef IS NOT NULL
    EXEC('ALTER TABLE dbo.Orden_Detalle DROP CONSTRAINT ' + @subDef);
GO

IF EXISTS (
    SELECT 1 FROM sys.computed_columns
    WHERE Name = 'subtotal' AND Object_ID = OBJECT_ID('dbo.Orden_Detalle')
)
BEGIN
    ALTER TABLE dbo.Orden_Detalle DROP COLUMN subtotal;
END
GO

/* ─── 5. Orden_Detalle: nuevas columnas ───────────────────────────────────── */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'precio_paquete_detal' AND Object_ID = OBJECT_ID('dbo.Orden_Detalle')
)
BEGIN
    /* Agregar como NULLABLE primero para no romper filas existentes */
    ALTER TABLE dbo.Orden_Detalle
        ADD precio_paquete_detal DECIMAL(12,2) NULL;
END
GO

/* Backfill: filas históricas no tenían referencia detal — usar el mismo precio_paquete */
UPDATE dbo.Orden_Detalle
SET    precio_paquete_detal = precio_paquete
WHERE  precio_paquete_detal IS NULL;
GO

ALTER TABLE dbo.Orden_Detalle
    ALTER COLUMN precio_paquete_detal DECIMAL(12,2) NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'aplica_mayorista' AND Object_ID = OBJECT_ID('dbo.Orden_Detalle')
)
BEGIN
    ALTER TABLE dbo.Orden_Detalle
        ADD aplica_mayorista BIT NOT NULL
            CONSTRAINT DF_OrdenDetalle_aplica_mayorista DEFAULT 0;
END
GO

/* ─── 6. Recrear columna computada subtotal = cantidad_paquetes * precio_paquete ── */
IF NOT EXISTS (
    SELECT 1 FROM sys.computed_columns
    WHERE Name = 'subtotal' AND Object_ID = OBJECT_ID('dbo.Orden_Detalle')
)
BEGIN
    ALTER TABLE dbo.Orden_Detalle
        ADD subtotal AS (cantidad_paquetes * precio_paquete) PERSISTED;
END
GO

COMMIT TRANSACTION;
GO

PRINT 'Migración 2026-05-25 aplicada correctamente.';
GO
