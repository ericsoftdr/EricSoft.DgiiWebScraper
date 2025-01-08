namespace EricSoft.DgiiWebScraper
{
    public class DgiiWebScraperOptions
    {
        public string UrlContribuyente {  get; set; }  
            = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/rnc.aspx";
        public string UrlCiudadanos { get; set; } 
            = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/ciudadanos.aspx";

        public ContribuyenteEtiqueta ContribuyenteEtiqueta { get; set; } = new ContribuyenteEtiqueta();
        public CiudadanoEtiqueta CiudadanoEtiqueta { get; set; } = new CiudadanoEtiqueta();

    }
}
