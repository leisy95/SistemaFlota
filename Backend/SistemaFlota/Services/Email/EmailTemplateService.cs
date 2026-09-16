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
        <body style='font-family: Arial; color:#333; font-size:14px'>

            <p>
                Buenos días,
            </p>

            <p>
                Adjunto orden de compra, de antemano muchas gracias
                por la atención prestada.
            </p>

            <p>
                <b>Orden de compra:</b> {numeroOrden}<br/>
                <b>Proveedor:</b> {proveedor}<br/>
                <b>Fecha:</b> {fecha:dd/MM/yyyy}
            </p>

            <p>
                <b>Confirmar recibido.</b>
            </p>

            <br/>

            <p>
                Cordialmente
            </p>

            <p>
                <b>Oliver Gutierrez</b><br/>
                Jefe de compras<br/>
                Empaques plásticos SAS
            </p>

        </body>
        </html>";
    }
}
