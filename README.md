# EricSoft.DgiiWebScraper

**EricSoft.DgiiWebScraper** es una pequeña librería para realizar consultas en línea a la página oficial de la Dirección General de Impuestos Internos (DGII) de la República Dominicana, con el fin de obtener datos de RNC o Cédula (por ejemplo, Nombre o Razón Social, Estado, Nombre Comercial, etc.).

## Características
- **Ligera**: El Scraping se hace solo con HttpClient (evita el uso de navegadores como lo hace Selenium, PuppeteerSharp u otras librerias).
- **Alta compatibilidad**: Este proyecto está desarrollado sobre .NET Standard 2.0, lo cual le otorga un amplio rango de compatibilidad con diversas plataformas de .NET, incluyendo: .NET Core 2.0 o superior, .NET 5 / .NET 6 / .NET 7, .NET Framework 4.6.1 o superior
Gracias a esto, puedes integrar este paquete en una amplia variedad de aplicaciones y proyectos sin problemas de compatibilidad.
- **Configuración flexible**: Personaliza las URLs y las etiquetas que la DGII usa en las tablas para obtener los resultados, si algo de esto cambia en el futuro, no debes esperar a que hagamos el cambio, puedes enviar los nuevos datos como parámetro.

## Instalación
```bash
dotnet add package EricSoft.DgiiWebScraper
```
## Ejemplos básicos
- **Consulta RNC o Cédula**
```csharp
//Cosulta básica
 string rnc = "131204783"; //Rnc o Cédula
 var scraper = new DgiiWebScraperClient();
 var result = await scraper.ConsultarRncCedulaAsync(rnc);
```
En esta consulta el único dato que se pasa como parámetro es el número de cédula o rnc, el metodo devuelve como respuesta un objeto de la clase:
```csharp
public class RncCedula
{
    public string Rnc { get; set; }
    public string Nombre { get; set; }
    public string NombreComercial { get; set; }
    public string Estado { get; set; }
}
````
Esta consulta busca el rnc o cédula en la sección de consulta de **RNC Contribuyentes** y si no lo encuentra lo busca en **RNC Registrados**. Para estos casos puntuales solo se devulven los campos comunes definidos en la clase anteriomente citada, si necesitas más datos podrías utilizar el método definido para cada sección, el cual devolvería un diccionario con todos los regsitros que figuran en la tabla html que se muestra en la página de la DGII al realizar la consulta. Los métodos para cada sección son **ConsultarContribuyenteAsync()** para "RNC Contribuyentes" y **ConsultarCiudadanoAsync()** para "RNC Registrados".

- **Consulta principal RNC Cotribuyentes**
```csharp
//Consulta principal RNC Cotribuyentes
 string rnc = "131204783"; //Rnc o Cédula
 var scraper = new DgiiWebScraperClient();
 var result = await scraper.ConsultarContribuyenteAsync(rnc);
````
Esta consulta devuelve un diccionario con los valores que se encuentran en la tabla HTML que se muestra en la página de la DGII al realizar una consulta en la sección Herramientas > Consultas > RNC Contribuyentes

- **Consulta principal RNC Registrados**
```csharp
//Consulta principal RNC Registrados
 string rnc = "131204783"; //Rnc o Cédula
 var scraper = new DgiiWebScraperClient();
 var result = await scraper.ConsultarCiudadanoAsync(rnc);
````
Esta consulta devuelve un diccionario con los valores que se encuentran en la tabla HTML que se muestra en la página de la DGII al realizar una consulta en la sección Herramientas > Consultas > RNC Registrados.

- **Consulta NCF**
```csharp
//Consulta NCF
 string rnc = "131204783"; //Rnc o Cédula
 string ncf = "BXXXXXXXXXX"; //Indique el NCF que desea consultar.
 var scraper = new DgiiWebScraperClient();
 var result = await scraper.ConsultarCiudadanoAsync(rnc,ncf);
````

Esta consulta devuelve un objeto de la siguiente clase:
```csharp
public class NcfResult
{
    public string RncCedula { get; set; }
    public string Nombre { get; set; }
    public string TipoComprobante { get; set; }
    public string Ncf { get; set; }
    public string Estado { get; set; }
    public string ValidoHasta { get; set; }
}
````
## Pasar parámetros
La manera de pasar parámetros a la case DgiiWebScraperClient() es por medio de su constructor, el cual recibe un objeto de la siguiente clase:
```csharp
public class DgiiWebScraperOptions
{
    public string UrlContribuyente {  get; set; }  
        = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/rnc.aspx";
    public string UrlCiudadanos { get; set; } 
        = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/ciudadanos.aspx";

    public string UrlNcf { get; set; }
        = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultasWeb/consultas/ncf.aspx";

    public ContribuyenteEtiqueta ContribuyenteEtiqueta { get; set; } = new ContribuyenteEtiqueta();
    public CiudadanoEtiqueta CiudadanoEtiqueta { get; set; } = new CiudadanoEtiqueta();
    public NcfEtiqueta NcfEtiqueta { get; set; } = new NcfEtiqueta();
}
````
Como se puede notar, cada propiedad tiene un valor por defecto, pero si alguno de estos cambia, podemos enviarlo como parámetro, por ejemplo, si la URL para consultar el RNC en la sección de contribuyentes cambia, podemos indicar la nueva URL de la siguiente manera
```csharp
var scraper = new DgiiWebScraperClient( new DgiiWebScraperOptions
{
    UrlContribuyente ="nuevaUrl" 
});
````
Esta flexibilidad de poder pasar parámetros no se limita a las URLs sino también a las etiquetas. Con el término "etiqueta" o "label" nos referimos a la primera columna que se muestra en la tabla HTML en la página de la DGII la cual contiene el resultado de nuestra consulta. Como comprenderás, para los métodos que no devuelven un diccionario, sino un objeto, es necesario poder vincular los datos de la tabla HTML con las propiedades de la clase, por ejemplo, está configurado por defuault que el nombre comercial del RNC consultado se encuentra en la tabla cuya fila tenga en su primera columna el texto "Nombre Comercial", si en el futuro la DGII devuelve un texto diferente entonces podemos configurarlo de la siguiente manera:
```csharp
var scraper = new DgiiWebScraperClient( new DgiiWebScraperOptions
{
    ContribuyenteEtiqueta = new ContribuyenteEtiqueta
    {
        NombreComercial = "Nuevo nombre" //Coloca el nuevo nombre de la etiqueta
    }
});
````
Si quieres hacer una prueba podrías jugar con los nombres de las etiquetas de la siguiente manera: te propongo enviemos los parámetros de manera tal que el estado del RNC figure en el campo NombreComercial y el nombre comercial en el campo Estado, eso lo podemos hacer de la siguiente manera:
```csharp
var scraper = new DgiiWebScraperClient( new DgiiWebScraperOptions
{
    ContribuyenteEtiqueta = new ContribuyenteEtiqueta
    {
        NombreComercial = "Estado",
        Estado = "Nombre Comercial"
    }
});
var result = await scraper.ConsultarRncCedulaAsync("131204783");
````

**Nota:** 
- Si está leyendo esta documentación en nuget u otro sitio, tome en consideración que la misma puede ser mejorada sin que esto implique publicar un release, por lo que sugerimos ver la documentación en el repositorio github https://github.com/ericsoftdr/EricSoft.DgiiWebScraper
-  Utilice esta librería de manera ética y respete los términos de uso del sitio web de la DGII.
