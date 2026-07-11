import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  const rutasPublicas = ['/Auth/login', '/Auth/recuperar', '/Auth/cambiar-password'];
  const esPublica = rutasPublicas.some(ruta => req.url.includes(ruta));

  const token = sessionStorage.getItem('token') || localStorage.getItem('token');

  const authReq = (token && !esPublica)
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !esPublica && token) {
        // Solo limpiar — sin reload. El usuario verá el login al navegar
        sessionStorage.clear();
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('rol');
      }
      return throwError(() => error);
    })
  );
};