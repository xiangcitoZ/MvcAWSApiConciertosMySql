using Microsoft.AspNetCore.Mvc;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using MvcAWSApiConciertosMySql.Services;
using Newtonsoft.Json;

namespace MvcAWSApiConciertosMySql.Controllers
{
    public class ConciertosController : Controller
    {
        private ServiceApiConciertos service;

        private string BucketUrl;
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;
        public ConciertosController(ServiceApiConciertos service
            , IConfiguration configuration)
        {

            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.service = service;
            this.BucketUrl = model.BucketUrl;
        }


        public async Task<IActionResult>  Index()
        {
            List<string> filesS3 = await this.service.GetVersionsFilesAsync();
            ViewData["BUCKETURL"] = this.BucketUrl;
            List<Eventos> eventos = await this.service.GetEventosAsync();
            return View(eventos);

        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Eventos evento, IFormFile file)
        {
            string filename = file.FileName;

            using (Stream stream = file.OpenReadStream())
            {
                await this.service.UploadFileAsync(file.FileName, stream);
            }
                evento.Imagen = filename;
                await this.service.CreateEvento(evento);
            return RedirectToAction("Index");
        }


        //BUCKETS IMAGENES

        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            using (Stream stream = file.OpenReadStream())
            {
                await this.service.UploadFileAsync(file.FileName, stream);
            }
            return RedirectToAction("Index");
        }


    }
}
