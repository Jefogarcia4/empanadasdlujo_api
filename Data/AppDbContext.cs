using EmpanadasDLujo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Sabor> Sabores { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<SKU> SKUs { get; set; }
    public DbSet<ListaPrecios> ListasPrecios { get; set; }
    public DbSet<PrecioSKU> PreciosSKU { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Orden> Ordenes { get; set; }
    public DbSet<OrdenDetalle> OrdenDetalles { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<ComboComponente> ComboComponentes { get; set; }
    public DbSet<WhatsAppLog> WhatsAppLogs { get; set; }
    public DbSet<CarritoWhatsApp> CarritosWhatsApp { get; set; }
    public DbSet<CarritoWhatsAppDetalle> CarritoWhatsAppDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Categoria: nombre unique
        modelBuilder.Entity<Categoria>()
            .HasIndex(c => c.Nombre)
            .IsUnique();

        // Sabor: nombre unique
        modelBuilder.Entity<Sabor>()
            .HasIndex(s => s.Nombre)
            .IsUnique();

        // ListaPrecios: nombre unique
        modelBuilder.Entity<ListaPrecios>()
            .HasIndex(lp => lp.Nombre)
            .IsUnique();

        // PrecioSKU: unique constraint (codigo_sku, id_lista)
        modelBuilder.Entity<PrecioSKU>()
            .HasIndex(p => new { p.CodigoSku, p.IdLista })
            .IsUnique();

        // Orden: check constraint estado
        modelBuilder.Entity<Orden>()
            .ToTable(tb => tb.HasCheckConstraint(
                "chk_orden_estado",
                "estado IN ('PENDIENTE','CONFIRMADA','ENTREGADA','ANULADA')"));

        // OrdenDetalle: check constraints
        //  - cantidad_paquetes > 0
        //  - un detalle referencia un SKU O un combo, nunca ambos ni ninguno
        modelBuilder.Entity<OrdenDetalle>()
            .ToTable(tb =>
            {
                tb.HasCheckConstraint("chk_cantidad", "cantidad_paquetes > 0");
                tb.HasCheckConstraint(
                    "chk_detalle_sku_o_combo",
                    "(codigo_sku IS NOT NULL AND id_combo IS NULL) OR (codigo_sku IS NULL AND id_combo IS NOT NULL)");
            });

        // OrdenDetalle → Combo: sin cascade para no borrar combos al eliminar órdenes
        modelBuilder.Entity<OrdenDetalle>()
            .HasOne(od => od.Combo)
            .WithMany(c => c.OrdenDetalles)
            .HasForeignKey(od => od.IdCombo)
            .OnDelete(DeleteBehavior.Restrict);

        // Combo: codigo_combo unique
        modelBuilder.Entity<Combo>()
            .HasIndex(c => c.CodigoCombo)
            .IsUnique();

        modelBuilder.Entity<Combo>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        // ComboComponente: cantidad_paquetes > 0 y unicidad (combo, sku)
        modelBuilder.Entity<ComboComponente>()
            .ToTable(tb => tb.HasCheckConstraint(
                "chk_combo_componente_cantidad",
                "cantidad_paquetes > 0"));

        modelBuilder.Entity<ComboComponente>()
            .HasIndex(cc => new { cc.IdCombo, cc.CodigoSku })
            .IsUnique();

        // ComboComponente → SKU: sin cascade
        modelBuilder.Entity<ComboComponente>()
            .HasOne(cc => cc.SKU)
            .WithMany()
            .HasForeignKey(cc => cc.CodigoSku)
            .OnDelete(DeleteBehavior.Restrict);

        // OrdenDetalle: subtotal is computed column
        modelBuilder.Entity<OrdenDetalle>()
            .Property(od => od.Subtotal)
            .HasComputedColumnSql("cantidad_paquetes * precio_paquete", stored: true);

        // SKU: activo default 1
        modelBuilder.Entity<SKU>()
            .Property(s => s.Activo)
            .HasDefaultValue(true);

        // Cliente: activo default 1
        modelBuilder.Entity<Cliente>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        // Orden: defaults
        modelBuilder.Entity<Orden>()
            .Property(o => o.FechaOrden)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Orden>()
            .Property(o => o.Estado)
            .HasDefaultValue("PENDIENTE");

        modelBuilder.Entity<Orden>()
            .Property(o => o.Total)
            .HasDefaultValue(0m);

        modelBuilder.Entity<Orden>()
            .Property(o => o.Subtotal)
            .HasDefaultValue(0m);

        modelBuilder.Entity<Orden>()
            .Property(o => o.Descuento)
            .HasDefaultValue(0m);

        // Cliente: guardar_info default 0
        modelBuilder.Entity<Cliente>()
            .Property(c => c.GuardarInfo)
            .HasDefaultValue(false);

        // OrdenDetalle: aplica_mayorista default 0
        modelBuilder.Entity<OrdenDetalle>()
            .Property(od => od.AplicaMayorista)
            .HasDefaultValue(false);

        // ── CarritoWhatsApp (carrito borrador generado desde WhatsApp) ──
        modelBuilder.Entity<CarritoWhatsApp>(e =>
        {
            e.HasIndex(c => c.Token).IsUnique();
            e.Property(c => c.Token).HasDefaultValueSql("NEWID()");
            e.Property(c => c.Estado).HasDefaultValue("ACTIVO");
            e.Property(c => c.FechaCreacion).HasDefaultValueSql("GETDATE()");
            e.ToTable(tb => tb.HasCheckConstraint(
                "chk_carrito_estado",
                "estado IN ('ACTIVO','CONVERTIDO')"));
        });

        // CarritoWhatsAppDetalle: mismas reglas que OrdenDetalle (cantidad > 0, SKU XOR combo).
        // FK al carrito con cascade: al borrar un carrito se borran sus detalles.
        modelBuilder.Entity<CarritoWhatsAppDetalle>(e =>
        {
            e.ToTable(tb =>
            {
                tb.HasCheckConstraint("chk_carrito_cantidad", "cantidad > 0");
                tb.HasCheckConstraint(
                    "chk_carrito_detalle_sku_o_combo",
                    "(codigo_sku IS NOT NULL AND id_combo IS NULL) OR (codigo_sku IS NULL AND id_combo IS NOT NULL)");
            });
            e.HasOne(d => d.Carrito)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.IdCarrito)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
