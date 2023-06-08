using Amazon.S3;
using Amazon.S3.Model;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MvcAWSApiConciertosMySql.Services
{
    public class ServiceApiConciertos
    {
        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApi;

        private string BucketName;
        //S3 TRABAJA CON UNA INTERFACE LLAMADA IAmazonS3
        private IAmazonS3 ClientS3;

        string miSecreto = HelperSecretManager.GetSecretAsync().Result;


        public ServiceApiConciertos(IConfiguration configuration
            , IAmazonS3 clientS3)
        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);


            this.Header =
                new MediaTypeWithQualityHeaderValue("application/json");
            //this.UrlApi =
            //    configuration.GetValue<string>("ApiUrls:ApiConciertos");

           this.UrlApi = model.ApiConciertos;
              

            this.BucketName = model.BucketName;
            this.ClientS3 = clientS3;
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                //LO UNICO QUE DEBEMOS TENER EN CUENTA ES 
                //QUE LAS PETICIONES, A VECES SE QUEDAN ATASCADAS
                //SI LAS HACEMOS MEDIANTE .BaseAddress + Request
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string url = this.UrlApi + request;
                HttpResponseMessage response =
                    await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task CreateEvento(Eventos evento)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Conciertos/CreateEventos";
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string jsonComic = JsonConvert.SerializeObject(evento);
                StringContent content =
                    new StringContent(jsonComic, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(this.UrlApi + request, content);
            }
        }

        public async Task<List<Eventos>> GetEventosAsync()
        {
            string request = "api/Conciertos/GetEventos";
            List<Eventos> eventos = await this.CallApiAsync<List<Eventos>>(request);
            return eventos;
        }

        public async Task<Eventos> FindEventosAsync(int id)
        {
            string request = "api/comics/EventoCategoria/" + id;
            Eventos eventos = await this.CallApiAsync<Eventos>(request);
            return eventos;
        }


        //BUCKETS IMAGENES

        //COMENZAMOS SUBIENDO FICHEROS AL BUCKET
        //NECESITAMOS FileName, Stream y un Key/Value
        public async Task<bool>
            UploadFileAsync(string fileName, Stream stream)
        {
            PutObjectRequest request = new PutObjectRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = this.BucketName
            };
            //DEBEMOS OBTENER UNA RESPUESTA CON EL MISMO TIPO 
            //DE REQUEST
            PutObjectResponse response = await
                this.ClientS3.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //METODO PARA RECUPERAR LAS VERSIONES DE UN FILE
        public async Task<List<string>> GetVersionsFilesAsync()
        {
            ListVersionsResponse response =
                await this.ClientS3.ListVersionsAsync(this.BucketName);
            List<string> versiones =
                response.Versions.Select(x => x.Key).ToList();
            return versiones;
        }

        //METODO PARA RECUPERAR UN FILE POR CODIGO
        public async Task<Stream> GetFileAsync(string fileName)
        {
            GetObjectResponse response =
                await this.ClientS3.GetObjectAsync(this.BucketName, fileName);
            return response.ResponseStream;
        }
    }


}

