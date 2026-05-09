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

        // OrdenDetalle: check constraint cantidad_paquetes > 0
        modelBuilder.Entity<OrdenDetalle>()
            .ToTable(tb => tb.HasCheckConstraint(
                "chk_cantidad",
                "cantidad_paquetes > 0"));

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
    }
}
