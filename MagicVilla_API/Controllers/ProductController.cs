using MagicVilla_API.Data;
using MagicVilla_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;

        public ProductController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext db)
        {
            _webHostEnvironment = webHostEnvironment;
            this._db = db;
        }


        [HttpPost("DataBaseMutilUploadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToDataBaseMutilUploadImage(IFormFileCollection formFileCillection, int productId)
        {
            foreach(var file in formFileCillection)
            {
                using(MemoryStream stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var villa = _db.Villas.FirstOrDefaultAsync(u=>u.Id == productId);
                    // update a new villa object and can add imagUrl or imageUrl list
                    // ListImage = stream.ToList(); or  ToArray()
                }
            }
            return Ok();
        }


        [HttpPut("UploadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadImage(IFormFile formFile, int productId)
        {
            try
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string filePath = GetFilePath(productId);
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }
                string imagePath = filePath + "\\" + fileName + ".png";

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using(FileStream stream = System.IO.File.Create(imagePath))
                {
                    await formFile.CopyToAsync(stream);
                    return Ok();
                }

            }
            catch (Exception)
            {

                throw;
            }

        }


        [NonAction]
        [ProducesResponseType(StatusCodes.Status200OK)]
        private string GetFilePath(int productId)
        {
            return _webHostEnvironment.WebRootPath + "\\Upload\\product\\" + productId;
        }


        [HttpPut("MutiUploadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MutilUploadImage(IFormFileCollection formFileCillection, int productId)
        {
            try
            {
                string filePath = GetFilePath(productId);
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                foreach(var file in formFileCillection)
                {
                    
                    string imagePath = filePath + "\\" + file.FileName;

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    using (FileStream stream = System.IO.File.Create(imagePath))
                    {
                        await file.CopyToAsync(stream);
                        return Ok();
                    }
                }  

            }
            catch (Exception)
            {
                throw;
            }
            
            return Ok();
        }


        [HttpGet("GetImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult GetImage(int productId)
        { 
            string ImageURL = string.Empty;
            string hostURL = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            try
            {
                string filePath = GetFilePath(productId);
                string imagePath = filePath + "\\" + productId + ".png";

                if (System.IO.File.Exists(imagePath))
                {
                    ImageURL = hostURL + "Upload/product/" + productId + "/" + productId + ".png";
                    
                }
                
            }
            catch (Exception)
            {

                throw;
            }
            return Ok(ImageURL);
        }


        [HttpGet("GetAllImages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  ActionResult GetAllImages(int productId)
        {
            List<string> ImageURL = new List<string>();
            string hostURL = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            try
            {
                string filePath = GetFilePath(productId);

                if (System.IO.Directory.Exists(filePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    foreach(FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagePath = filePath + "\\" + filename;

                        if (System.IO.File.Exists(imagePath))
                        {
                            var imageURL = hostURL + "/Upload/product/" + productId + "/" + filename;
                            ImageURL.Add(imageURL);
                        }

                    }

                }                
            }
            catch (Exception)
            {
                throw;
            }

            return Ok(ImageURL);
        }


        [HttpGet("DownloadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DownloadImage(int productId)
        {
            try
            {
                string filePath = GetFilePath(productId);
                string imagePath = filePath + "\\" + productId + ".png";

                if (System.IO.File.Exists(imagePath))
                {
                    MemoryStream stream = new MemoryStream();

                    using(FileStream fileStream = new FileStream(imagePath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                    stream.Position = 0;
                    // File(Stream, FileType, FileName)
                    return File(stream, "img/png", productId + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
            
        }


        [HttpGet("RemoveImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveImage(int productId)
        {
            try
            {
                string filePath = GetFilePath(productId);
                string imagePath = filePath + "\\" + productId + ".png";

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
            
        }


        [HttpGet("RemoveALLImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveALLImage(int productId)
        {
            try
            {
                string filePath = GetFilePath(productId);


                if (System.IO.Directory.Exists(filePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    foreach (FileInfo fileInfo in fileInfos)
                    {                      
                        fileInfo.Delete();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
