using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CatalogoController : ControllerBase
{
    private readonly AppDbContext _context;

    public CatalogoController(AppDbContext context) => _context = context;

    /// <summary>
    /// Devuelve todos los SKUs con su información completa (categoría, producto, sabor,
    /// gramaje, unidades) y los precios de cada lista de precios activa.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogoSkuDto>>> GetCatalogo(
        [FromQuery] bool? activo = null,
        [FromQuery] int? idCategoria = null)
    {
        var skuQuery = _context.SKUs
            .Include(s => s.Producto).ThenInclude(p => p.Categoria)
            .Include(s => s.Sabor)
            .Include(s => s.Precios).ThenInclude(p => p.ListaPrecios)
            .AsQueryable();

        if (activo.HasValue)
            skuQuery = skuQuery.Where(s => s.Activo == activo.Value);

        if (idCategoria.HasValue)
            skuQuery = skuQuery.Where(s => s.Producto.IdCategoria == idCategoria.Value);

        var skus = await skuQuery
            .OrderBy(s => s.Producto.Categoria.Nombre)
            .ThenBy(s => s.Producto.Nombre)
            .ThenBy(s => s.CodigoSku)
            .ToListAsync();

        var resultado = skus.Select(s => new CatalogoSkuDto
        {
            CodigoSku = s.CodigoSku,
            Categoria = s.Producto.Categoria.Nombre,
            Producto = s.Producto.Nombre,
            Sabor = s.Sabor.Nombre,
            GramajeG = s.GramajeG,
            UnidadesPorPaquete = s.UnidadesPorPaquete,
            Activo = s.Activo,
            Precios = s.Precios
                .OrderBy(p => p.IdLista)
                .Select(p => new CatalogoPrecioDto
                {
                    NombreLista = p.ListaPrecios.Nombre,
                    PrecioPaquete = p.PrecioPaquete,
                    PrecioPorUnidad = p.PrecioPorUnidad
                }).ToList()
        }).ToList();

        return Ok(resultado);
    }

    /// <summary>
    /// Devuelve el catálogo filtrado por una lista de precios específica.
    /// Útil para mostrar solo los precios de una lista (ej: PVxM o PVxD).
    /// </summary>
    [HttpGet("por-lista/{idLista:int}")]
    public async Task<ActionResult<IEnumerable<CatalogoSkuDto>>> GetCatalogoPorLista(
        int idLista,
        [FromQuery] bool? activo = null)
    {
        if (!await _context.ListasPrecios.AnyAsync(l => l.IdLista == idLista))
            return NotFound("Lista de precios no encontrada.");

        var skuQuery = _context.SKUs
            .Include(s => s.Producto).ThenInclude(p => p.Categoria)
            .Include(s => s.Sabor)
            .Include(s => s.Precios.Where(p => p.IdLista == idLista))
                .ThenInclude(p => p.ListaPrecios)
            .Where(s => s.Precios.Any(p => p.IdLista == idLista))
            .AsQueryable();

        if (activo.HasValue)
            skuQuery = skuQuery.Where(s => s.Activo == activo.Value);

        var skus = await skuQuery
            .OrderBy(s => s.Producto.Categoria.Nombre)
            .ThenBy(s => s.Producto.Nombre)
            .ThenBy(s => s.CodigoSku)
            .ToListAsync();

        var resultado = skus.Select(s => new CatalogoSkuDto
        {
            CodigoSku = s.CodigoSku,
            Categoria = s.Producto.Categoria.Nombre,
            Producto = s.Producto.Nombre,
            Sabor = s.Sabor.Nombre,
            GramajeG = s.GramajeG,
            UnidadesPorPaquete = s.UnidadesPorPaquete,
            Activo = s.Activo,
            Precios = s.Precios.Select(p => new CatalogoPrecioDto
            {
                NombreLista = p.ListaPrecios.Nombre,
                PrecioPaquete = p.PrecioPaquete,
                PrecioPorUnidad = p.PrecioPorUnidad
            }).ToList()
        }).ToList();

        return Ok(resultado);
    }
}
