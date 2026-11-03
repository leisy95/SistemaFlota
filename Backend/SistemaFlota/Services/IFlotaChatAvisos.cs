namespace SistemaFlota
{
    
    public interface IFlotaChatAvisos
    {
       
        Task<int?> PublicarEnGrupoAsync(string categoria, string texto);

       
        Task<int?> EnviarAUsuarioAsync(int flotaChatUsuarioId, string texto, string categoria);
    }
}
