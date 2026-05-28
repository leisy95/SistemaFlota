import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError }                           from 'rxjs/operators';
import { throwError }                           from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  const token = localStorage.getItem('token');

  // Si hay token, clona la request y agrega el header Authorization
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        console.error('🔒 401 — token vencido o inválido');
        // Opcional: redirigir al login
        // window.location.href = '/login';
      }
      return throwError(() => error);
    })
  );
};