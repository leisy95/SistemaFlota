interface UsuarioForm {
    username: string;
    password: string;
    nombres: string;
    apellidos: string;
    telefono: string;
    rol: string;
    email: string;
    activo: boolean;
    permisos: PermisoGranular[];
}