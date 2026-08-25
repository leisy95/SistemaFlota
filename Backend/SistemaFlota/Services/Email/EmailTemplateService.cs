namespace SistemaFlota.Services.Email;

public class EmailTemplateService
{
    public string RecepcionMercancia(
        string numeroRecepcion,
        string proveedor,
        DateTime fecha)
    {
        return $@"
        <html>
        <body style='font-family: Arial; color:#333'>

            <h2 style='color:#15803D'>
                Recepción de mercancía registrada
            </h2>

            <p>
                Se ha generado una nueva recepción de mercancía.
            </p>

            <table>
                <tr>
                    <td><b>Número:</b></td>
                    <td>{numeroRecepcion}</td>
                </tr>

                <tr>
                    <td><b>Proveedor:</b></td>
                    <td>{proveedor}</td>
                </tr>

                <tr>
                    <td><b>Fecha:</b></td>
                    <td>{fecha:dd/MM/yyyy HH:mm}</td>
                </tr>
            </table>

            <br/>

            <p>
                Se adjunta el documento PDF correspondiente.
            </p>

            <hr/>

            <small>
                Sistema Flota
            </small>

        </body>
        </html>";
    }

    public string OrdenCompra(
        string numeroOrden,
        string proveedor,
        DateTime fecha)
    {
        return $@"
        <html>
        <body style='font-family: Arial; color:#333'>

            <h2 style='color:#1D4ED8'>
                Orden de compra
            </h2>

            <p>
                Se ha generado una nueva orden de compra.
            </p>

            <table>
                <tr>
                    <td><b>Número:</b></td>
                    <td>{numeroOrden}</td>
                </tr>

                <tr>
                    <td><b>Proveedor:</b></td>
                    <td>{proveedor}</td>
                </tr>

                <tr>
                    <td><b>Fecha:</b></td>
                    <td>{fecha:dd/MM/yyyy HH:mm}</td>
                </tr>
            </table>

            <br/>

            <p>
                Se adjunta el documento PDF correspondiente.
            </p>

            <hr/>

            <small>
                Sistema Flota
            </small>

        </body>
        </html>";
    }
}