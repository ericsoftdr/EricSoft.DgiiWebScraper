# EricSoft.DgiiWebScraper

**EricSoft.DgiiWebScraper** es una pequeña librería para realizar consultas en línea a la página oficial de la Dirección General de Impuestos Internos (DGII) de la República Dominicana, con el fin de obtener datos de RNC o Cédula (por ejemplo, Nombre o Razón Social, Estado, Nombre Comercial, etc.).

## Características
- **Ligera**: El Scraping se hace solo con HttpClient (evita el uso de navegadores como lo hace Selenium u otras librerias).
- **Alta compatibilidad**: Este proyecto está desarrollado sobre .NET Standard 2.0, lo cual le otorga un amplio rango de compatibilidad con diversas plataformas de .NET, incluyendo: .NET Core 2.0 o superior, .NET 5 / .NET 6 / .NET 7, .NET Framework 4.6.1 o superior
Gracias a esto, puedes integrar este paquete en una amplia variedad de aplicaciones y proyectos sin problemas de compatibilidad.
- **Configuración flexible**: Personaliza las URLs y las etiquetas que la DGII usa en las tablas para obtener los resultados, si algo de esto cambia en el futuro, no debes esperar a que hagamos el cambio, puedes enviar los nuevos datos como parámetro.

## Instalación
```bash
dotnet add package EricSoft.DgiiWebScraper
```
## Ejemplos básicos
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
