using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindWebAPI.Data;
using NorthwindWebAPI.Models;

namespace NorthwindWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly NorthwindContext _context;

        public ProductsController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        /// <summary>
        /// ////////////////////////
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("ProductXID&Anio")]
        public IEnumerable<Object> Productosdefecha3()
        {

            var result = (
            from p in _context.Products
            from d in _context.Movementdetails
            from m in _context.Movements
            where p.ProductId == d.ProductId
            where m.MovementId == d.MovementId
            select new
            {
                m.Date,
                d.Quantity,
            }).GroupBy(e => e.Date.Month).Select(e => new
            {
                mes = e.Key,
                Cantidad = e.Sum(g => g.Quantity)

            }).Take(5);
            return result;
        }

        [HttpGet]
        [Route("Meses/{id}/{ano}")]
        public IEnumerable<Object> Productosdefecha(int id, int ano)
        {
            int idpro = id;
            int YearStart = ano;

            var result = (
                          from m in _context.Movements
                          from d in _context.Movementdetails
                          where m.MovementId == d.MovementId &&
                          d.ProductId == idpro && m.Date.Year == YearStart
                          select new
                          {
                              m.Date.Month,
                              d.Quantity
                          }
                          ).GroupBy(e => e.Month).Select(e => new
                          {
                              Mes = e.Key,
                              Cantidad = e.Sum(g => g.Quantity)
                          });
            return result;
        }

        [HttpGet]
        [Route("montoXmes")]
        public IEnumerable<Object> productosdefecha5()
        {

            var result = (
            from p in _context.Products
            from d in _context.Movementdetails
            from m in _context.Movements
            from w in _context.Warehouses
            where p.ProductId == d.ProductId

            where m.MovementId == d.MovementId

            where w.WarehouseId == m.OriginWarehouseId

            select new
            {
                m.Date,
                V = p.UnitPrice * d.Quantity
            }).GroupBy(e => e.Date.Month).Select(e => new
            {
                Anio = e.Key,
                Cantidad = e.Sum(g => g.V)
            });

            return result;
        }

        [HttpGet]
        [Route("montoXalmacen/{id}/{ware}")]
        public IEnumerable<Object> productosdefecha6(int id, int ware)
        {
            
            var idpro = id;
            var alma = ware;
            var result = (
            from p in _context.Products
            from d in _context.Movementdetails
            from m in _context.Movements
            from w in _context.Warehouses
            where p.ProductId == d.ProductId &&
            m.MovementId == d.MovementId &&
            w.WarehouseId == m.OriginWarehouseId &&
            p.ProductId == idpro &&
            m.OriginWarehouseId == alma

            select new
            {
                m.Date.Year,
                m.Date,
                V = p.UnitPrice * d.Quantity
            }).GroupBy(e => e.Date).Select(e => new
            {
                fecha = e.Key,
                mes = e.Key.Month,
                anio = e.Key.Year,
                cantidad = e.Sum(g => g.V)
               
            });

            return result;
        }
        //trimestre 1
        [HttpGet]
        [Route("Trimestre1Top5")]
        public IEnumerable<Object> cuatrimestre1()
        {

            var movementsdetails = _context.Movementdetails.ToList();
            var products = _context.Products.ToList();
            var movements = _context.Movements.ToList();

            var data = movementsdetails.Where(x => x.Movement.Date >= new DateTime(1997, 1, 1) && x.Movement.Date <= new DateTime(1997, 3, 31)).ToList();


            //sum
            var result = products.Join(data,
                            p => p.ProductId,
                            d => d.ProductId,
                            (p, d) => new
                            {
                                p.ProductName,
                                d.Quantity
                            }).GroupBy(e => e.ProductName).Select(e => new
                            {
                                ProductName = e.Key,
                                Cantidad = e.Sum(g => g.Quantity)
                            }).OrderByDescending(e => e.Cantidad).Take(5);



            return result;
        }
        //trimestre 2
        [HttpGet]
        [Route("Trimestre2Top5")]
        public IEnumerable<Object> cuatrimestre2()
        {

            var movementsdetails = _context.Movementdetails.ToList();
            var products = _context.Products.ToList();
            var movements = _context.Movements.ToList();

            var data = movementsdetails.Where(x => x.Movement.Date >= new DateTime(1997, 4, 1) && x.Movement.Date <= new DateTime(1997, 6, 30)).ToList();


            //sum
            var result = products.Join(data,
                            p => p.ProductId,
                            d => d.ProductId,
                            (p, d) => new
                            {
                                p.ProductName,
                                d.Quantity
                            }).GroupBy(e => e.ProductName).Select(e => new
                            {
                                ProductName = e.Key,
                                Cantidad = e.Sum(g => g.Quantity)
                            }).OrderByDescending(e => e.Cantidad).Take(5);



            return result;
        }
        //trimestre 3
        [HttpGet]
        [Route("Trimestre3Top5")]
        public IEnumerable<Object> cuatrimestre3()
        {
            var movementsdetails = _context.Movementdetails.ToList();
            var products = _context.Products.ToList();
            var movements = _context.Movements.ToList();

            var data = movementsdetails.Where(x => x.Movement.Date >= new DateTime(1997, 7, 1) && x.Movement.Date <= new DateTime(1997, 9, 30)).ToList();


            //sum
            var result = products.Join(data,
                            p => p.ProductId,
                            d => d.ProductId,
                            (p, d) => new
                            {
                                p.ProductName,
                                d.Quantity
                            }).GroupBy(e => e.ProductName).Select(e => new
                            {
                                ProductName = e.Key,
                                Cantidad = e.Sum(g => g.Quantity)
                            }).OrderByDescending(e => e.Cantidad).Take(5);



            return result;
        }
        //trimestre 4
        [HttpGet]
        [Route("Trimestre4Top5")]
        public IEnumerable<Object> cuatrimestre4()
        {

            var movementsdetails = _context.Movementdetails.ToList();
            var products = _context.Products.ToList();
            var movements = _context.Movements.ToList();

            var data = movementsdetails.Where(x => x.Movement.Date >= new DateTime(1997, 10, 1) && x.Movement.Date <= new DateTime(1997, 12, 31)).ToList();

            
            //sum
            var result = products.Join(data,
                            p => p.ProductId,
                            d => d.ProductId,
                            (p, d) => new
                            {
                                p.ProductName,
                                d.Quantity
                            }).GroupBy(e => e.ProductName).Select(e => new
                            {
                                ProductName = e.Key,
                                Cantidad = e.Sum(g => g.Quantity)
                            }).OrderByDescending(e => e.Cantidad).Take(5);



            return result;
        }



    }
}
