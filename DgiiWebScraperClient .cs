using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EricSoft.DgiiWebScraper
{

    public class DgiiWebScraperClient
    {
        private readonly HttpClient _httpClient;
        private readonly DgiiWebScraperOptions _options;

        public DgiiWebScraperClient(DgiiWebScraperOptions options = null)
        {            
            _options = options ?? new DgiiWebScraperOptions();

            // Manejo de cookies para mantener la sesión
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true
            };

            _httpClient = new HttpClient(handler);

            //Simular un User-Agent de navegador
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36");
        }

        /// <summary>
        /// Consulta el RNC o Cédula en la sección de Contribuyentes
        /// </summary>
        public async Task<Dictionary<string, string>> ConsultarContribuyenteAsync(string rnc)
        {
            return await GetScrapingResultAsync(
                url: _options.UrlContribuyente,
                rnc: rnc,
                buildPostData: (r, viewState, eventValidation, viewStateGenerator) =>
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("ctl00$smMain", "ctl00$cphMain$upBusqueda|ctl00$cphMain$btnBuscarPorRNC"),
                        new KeyValuePair<string,string>("ctl00$cphMain$txtRNCCedula", r),
                        new KeyValuePair<string,string>("ctl00$cphMain$txtRazonSocial", ""),
                        new KeyValuePair<string,string>("ctl00$cphMain$hidActiveTab", ""),
                        new KeyValuePair<string,string>("__EVENTTARGET", "ctl00$cphMain$btnBuscarPorRNC"),
                        new KeyValuePair<string,string>("__EVENTARGUMENT", ""),
                        new KeyValuePair<string,string>("__VIEWSTATE", viewState),
                        new KeyValuePair<string,string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                        new KeyValuePair<string,string>("__EVENTVALIDATION", eventValidation),
                        new KeyValuePair<string,string>("__ASYNCPOST", "true"),
                    },
                dataTableXPath: "//table[@id='cphMain_dvDatosContribuyentes']"
            );
        }

        /// <summary>
        /// Consulta el RNC o Cédula en la sección de RNC Registrados
        /// </summary>
        public async Task<Dictionary<string, string>> ConsultarCiudadanoAsync(string rnc)
        {
            return await GetScrapingResultAsync(
                url: _options.UrlCiudadanos,
                rnc: rnc,
                buildPostData: (r, viewState, eventValidation, viewStateGenerator) =>
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("ctl00$smMain", "ctl00$cphMain$upBusqueda|ctl00$cphMain$btnBuscarCedula"),
                        new KeyValuePair<string,string>("__EVENTTARGET", ""),
                        new KeyValuePair<string,string>("__EVENTARGUMENT", ""),
                        new KeyValuePair<string,string>("__VIEWSTATE", viewState),
                        new KeyValuePair<string,string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                        new KeyValuePair<string,string>("__EVENTVALIDATION", eventValidation),
                        new KeyValuePair<string,string>("ctl00$cphMain$txtCedula", r),
                        new KeyValuePair<string,string>("ctl00$cphMain$btnBuscarCedula", "Buscar"),
                        new KeyValuePair<string,string>("__ASYNCPOST", "true"),
                    },
                dataTableXPath: "//table[@id='cphMain_dvResultadoCedula']"
            );
        }

        /// <summary>
        /// Consulta el RNC o Cedula en la seccion de consulta de Contribuyentes y si no lo consigue lo busca la sección de RNC Registrado.
        /// </summary>
        public async Task<RncCedula> ConsultarRncCedulaAsync(string rncCedula)
        {
            RncCedula result = null;

            var contribuyenteResponse = await ConsultarContribuyenteAsync(rncCedula);

            if (contribuyenteResponse.Count > 0)
            {
                result = new RncCedula
                {
                    Rnc = rncCedula,
                    Nombre = contribuyenteResponse.ContainsKey(_options.ContribuyenteEtiqueta.Nombre) 
                             ? contribuyenteResponse[_options.ContribuyenteEtiqueta.Nombre] : "",
                    NombreComercial = contribuyenteResponse.ContainsKey(_options.ContribuyenteEtiqueta.NombreComercial) 
                                      ? contribuyenteResponse[_options.ContribuyenteEtiqueta.NombreComercial] : "",
                    Estado = contribuyenteResponse.ContainsKey(_options.ContribuyenteEtiqueta.Estado) 
                             ? contribuyenteResponse[_options.ContribuyenteEtiqueta.Estado] : ""
                };
            }
            else
            {
                var cuidadanoResponse = await ConsultarCiudadanoAsync(rncCedula);

                if (cuidadanoResponse.Count > 0)
                {
                    result = new RncCedula
                    {
                        Rnc = rncCedula,
                        Nombre = cuidadanoResponse.ContainsKey(_options.CiudadanoEtiqueta.Nombre) 
                                 ? cuidadanoResponse[_options.CiudadanoEtiqueta.Nombre] : "",
                        NombreComercial = "",
                        Estado = cuidadanoResponse.ContainsKey(_options.CiudadanoEtiqueta.Estado) 
                                 ? cuidadanoResponse[_options.CiudadanoEtiqueta.Estado] : "",
                    };
                }
            }

            return result;

        }

        /// <summary>
        /// Consulta el lisado de RNC o Cedula en la seccion de consulta de Contribuyentes y si no lo consigue lo busca la sección de RNC Registrado.
        /// </summary>
        public async Task<List<RncCedula>> ConsultarListaRncCedulaAsync(string[] rncCedulas)
        {
            var result = new List<RncCedula>();

            foreach (var rnc in rncCedulas)
            {
                var response = await ConsultarRncCedulaAsync(rnc);
                if (response != null)
                    result.Add(response);

                await Task.Delay(50);
            }

            return result;
        }

        /// <summary>
        /// Consulta ncf
        /// </summary>
        public async Task<NcfResult> ConsultarNcfAsync(string rnc, string ncf)
        {
            NcfResult result = null;
            
            if (string.IsNullOrWhiteSpace(rnc))
                throw new Exception("Especifique el RNC");

            if (string.IsNullOrWhiteSpace(ncf))
                throw new Exception("Especifique el NCF");

            rnc = rnc.Trim().Replace("-", "");
            ncf = ncf.Trim();

            var data = await GetScrapingResultAsync(
               url: _options.UrlNcf,
               rnc: rnc,
               buildPostData: (r, viewState, eventValidation, viewStateGenerator) =>
                   new List<KeyValuePair<string, string>>
                   {
                        new KeyValuePair<string,string>("ctl00$smMain", "ctl00$upMainMaster|ctl00$cphMain$btnConsultar"),
                        new KeyValuePair<string,string>("ctl00$cphMain$txtRNC", r),
                        new KeyValuePair<string,string>("ctl00$cphMain$txtNCF", ncf),
                        new KeyValuePair<string,string>("ctl00$cphMain$btnConsultar", "Buscar"),
                        new KeyValuePair<string,string>("__EVENTTARGET", ""),
                        new KeyValuePair<string,string>("__EVENTARGUMENT", ""),
                        new KeyValuePair<string,string>("__VIEWSTATE", viewState),
                        new KeyValuePair<string,string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                        new KeyValuePair<string,string>("__EVENTVALIDATION", eventValidation),
                        new KeyValuePair<string,string>("__ASYNCPOST", "true"),
                   },
               dataTableXPath: "//div[@id='cphMain_pResultado']//table[contains(@class,'detailview')]"
                               
           ) ;

            if ( data != null  && data.Count > 0)
            {
                result = new NcfResult
                {
                    RncCedula = data.ContainsKey(_options.NcfEtiqueta.RncCedula)
                                ? data[_options.NcfEtiqueta.RncCedula] : "",
                    Nombre = data.ContainsKey(_options.NcfEtiqueta.Nombre)
                             ? data[_options.NcfEtiqueta.Nombre] : "",
                    TipoComprobante = data.ContainsKey(_options.NcfEtiqueta.TipoComprobante)
                                     ? data[_options.NcfEtiqueta.TipoComprobante] : "",
                    Ncf = data.ContainsKey(_options.NcfEtiqueta.Ncf)
                          ? data[_options.NcfEtiqueta.Ncf] : "",
                    Estado = data.ContainsKey(_options.NcfEtiqueta.Estado)
                          ? data[_options.NcfEtiqueta.Estado] : "",
                    ValidoHasta = data.ContainsKey(_options.NcfEtiqueta.ValidoHasta)
                          ? data[_options.NcfEtiqueta.ValidoHasta] : "",
                };
            }

            return result;
        }

        /// <summary>
        /// Método auxiliar genérico para evitar duplicar lógica en cada consulta.
        /// </summary>
        private async Task<Dictionary<string, string>> GetScrapingResultAsync(
            string url,
            string rnc,
            Func<string, string, string, string, List<KeyValuePair<string, string>>> buildPostData,
            string dataTableXPath)
        {
            var result = new Dictionary<string, string>();

            rnc = rnc.Trim().Replace("-", "");

            // 1) GET a la página
            var getResponse = await _httpClient.GetAsync(url);

            if (!getResponse.IsSuccessStatusCode)
                return result; // O lanzar excepción, según tu preferencia.

            var initialHtml = await getResponse.Content.ReadAsStringAsync();

            // 2) Parsear el HTML para extraer campos ocultos
            var doc = new HtmlDocument();
            doc.LoadHtml(initialHtml);

            string viewState = GetInputValue(doc, "__VIEWSTATE");
            string eventValidation = GetInputValue(doc, "__EVENTVALIDATION");
            string viewStateGenerator = GetInputValue(doc, "__VIEWSTATEGENERATOR");

            // 3) Construir el POST usando la función buildPostData
            var postData = buildPostData(rnc, viewState, eventValidation, viewStateGenerator);
            var formContent = new FormUrlEncodedContent(postData);

            // 4) POST
            var postResponse = await _httpClient.PostAsync(url, formContent);
            if (!postResponse.IsSuccessStatusCode)
                return result; 

            // 5) Parsear la respuesta final y extraer datos
            var responseHtml = await postResponse.Content.ReadAsStringAsync();
            result = GetData(responseHtml, dataTableXPath);

            return result;
        }

        /// <summary>
        /// Utilidad para extraer el valor de un input hidden por su ID
        /// </summary>
        private string GetInputValue(HtmlDocument doc, string inputId)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//input[@id='{inputId}']");
            return node?.GetAttributeValue("value", "") ?? "";
        }

        private Dictionary<string, string> GetData(string responseHtml, string tableSelector)
        {

            //Cargar el HTML en HtmlAgilityPack
            var docResponse = new HtmlDocument();
            docResponse.LoadHtml(responseHtml);

            //Seleccionar la tabla por su 'id'
            var tableNode = docResponse.DocumentNode.SelectSingleNode(tableSelector);

            if (tableNode == null)
                return new Dictionary<string, string>();

            //Recorrer filas y extraer datos
            var rows = tableNode.SelectNodes(".//tr");

            // Usamos un diccionario para almacenar los pares "Etiqueta -> Valor"
            var dataDict = new Dictionary<string, string>();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td|th");
                    if (cells != null && cells.Count == 2)
                    {
                        // Decodificamos el HTML para quitar entidades como &#243;, &#233;, etc.
                        string label = WebUtility.HtmlDecode(cells[0].InnerText).Trim();
                        string value = WebUtility.HtmlDecode(cells[1].InnerText).Trim();

                        dataDict[label] = value;
                    }
                }
            }

            return dataDict;
        }
    }
}
