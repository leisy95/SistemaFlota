using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.Auditorias.Base
{
    public abstract class BaseService
    {
        private readonly SistemaFlota.AuditoriaService _auditoria;
        private readonly ICurrentUserService _currentUser;

        protected BaseService(
            SistemaFlota.AuditoriaService auditoria,
            ICurrentUserService currentUser)
        {
            _auditoria = auditoria;
            _currentUser = currentUser;
        }

        protected Task AuditarAsync(
            string accion,
            string modulo,
            string detalle,
            int? registroId = null)
        {
            return _auditoria.RegistrarAsync(
                usuario: _currentUser.Usuario ?? _currentUser.Nombre ?? "Desconocido",
                rol: _currentUser.Rol ?? "Desconocido",
                accion: accion,
                modulo: modulo,
                detalle: detalle,
                registroId: registroId);
        }
    }
}