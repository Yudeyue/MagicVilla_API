using ClosedXML.Excel;
using MagicVilla_API.Data;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace MagicVilla_API.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/VillaAPI")]
    // ApiContriller support ModelState validation
    [ApiController]
    //[EnableCors("CorsPolicy")]
    //[DisableCors]
    //[EnableRateLimiting("Fixed Window")]
    //[Authorize]
    //[AllowAnonymous] // cancel the authorized can be used for individual compoment
    public class VillaAPIController: ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ILogger<VillaAPIController> _logger { get; }

        public VillaAPIController(ApplicationDbContext db, ILogger<VillaAPIController> logger, IWebHostEnvironment webHostEnvironment) 
        {
            _db = db;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.LogInformation("Getting all villas");
            return Ok(_db.Villas.ToList());
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(VillaDTO))]
        //[ProducesResponseType(200, Type =typeof(VillaDTO))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Get villa error with id " + id);
                return BadRequest();
            }
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(VillaDTO))]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            //if (!ModelState.IsValid)
            //{
              //  return BadRequest(ModelState);
            //}
            if (_db.Villas.FirstOrDefault(u=>u.Name.ToLower()== villaDTO.Name.ToLower()) != null) {
                ModelState.AddModelError("CustomError", "Villa already exists!");
                return BadRequest(ModelState);
            }

            if (villaDTO == null)
            {
                return BadRequest();
            }
            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //villa.Id = _db.Villas.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            Villa newVilla = new ()
            {
                Id = villaDTO.Id,
                Name = villaDTO.Name,
                Details = villaDTO.Details,
                Amenity = villaDTO.Amenity,
                ImageUrl = villaDTO.ImageUrl,
                Occupany = villaDTO.Occupany,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft,
            };
            _db.Villas.Add(newVilla);
            _db.SaveChanges();

            // create a new route that could get the new created item.
            return CreatedAtRoute("GetVilla", new {Id = villaDTO.Id}, villaDTO);
        }


        [HttpDelete("{id:int}", Name ="DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            
            var villa = _db.Villas.FirstOrDefault(u=>u.Id == id);

            if (villa == null)
                return NotFound();

            _db.Villas.Remove(villa);
            _db.SaveChanges();
            return NoContent();    
        }


        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Update(int id, [FromBody] VillaDTO villaDTO)
        {
            if (id != villaDTO.Id || villaDTO == null)
                return BadRequest();
            // tell entityframework to not track the willa(id) object because it should retrevice the newVilla.
            var villa = _db.Villas.AsNoTracking().FirstOrDefault(u => u.Id == id);

            if (villa == null)
                return NotFound();

            Villa newVilla = new()
            {
                Id = villaDTO.Id,
                Name = villaDTO.Name,
                Details = villaDTO.Details,
                Amenity = villaDTO.Amenity,
                ImageUrl = villaDTO.ImageUrl,
                Occupany = villaDTO.Occupany,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft,
            };

            _db.Villas.Update(newVilla);
            _db.SaveChanges(true);  

            return NoContent();
        }


        [HttpPatch("{id:int}", Name = "PatchPartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        {
            if (id == 0 || patchDTO == null)
                return BadRequest();
            // tell entityframework to not track the id of villa
            var villa = _db.Villas.AsNoTracking().FirstOrDefault(u => u.Id == id);

            if (villa == null)
                return NotFound();

            VillaDTO newVillaDTO = new()
            {
                Id = villa.Id,
                Name = villa.Name,
                Details = villa.Details,
                Amenity = villa.Amenity,
                ImageUrl = villa.ImageUrl,
                Occupany = villa.Occupany,
                Rate = villa.Rate,
                Sqft = villa.Sqft,
            };

            // apply changes to villa and store in ModeState
            patchDTO.ApplyTo(newVillaDTO, ModelState);

            Villa newVilla = new()
            {
                Id = newVillaDTO.Id,
                Name = newVillaDTO.Name,
                Details = newVillaDTO.Details,
                Amenity = newVillaDTO.Amenity,
                ImageUrl = newVillaDTO.ImageUrl,
                Occupany = newVillaDTO.Occupany,
                Rate = newVillaDTO.Rate,
                Sqft = newVillaDTO.Sqft,
            };

            _db.Villas.Update(newVilla);
            _db.SaveChanges();  

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent() ;    
        }


        // install package closedXML
        [HttpGet("ExportExcelFile")]
        public ActionResult ExportExcelFile()
        {
            var filePath = _webHostEnvironment.WebRootPath + "\\Export";
            var excelFilePath = filePath + "\\Villas.xlsx";

            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Details", typeof(string));
            dt.Columns.Add("Rate", typeof(int));
            dt.Columns.Add("Sqft", typeof(int));
            dt.Columns.Add("Occupany", typeof(int));

            var data = _db.Villas.ToList();

            if (data != null && data.Count > 0)
            {
                data.ForEach(item =>
                {
                    dt.Rows.Add(item.Id,item.Name,item.Details,item.Rate,item.Sqft,item.Occupany);
                });
            }

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Villas");

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        if (System.IO.File.Exists(excelFilePath))
                        {
                            System.IO.File.Delete(excelFilePath);
                        }

                        wb.SaveAs(excelFilePath);

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Villas.xlsx");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

    }
}
