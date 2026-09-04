import {
  HttpInterceptorFn,
  HttpErrorResponse
} from '@angular/common/http';

import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  // RUTAS PÚBLICAS

  const rutasPublicas = [
    '/Auth/login',
    '/Auth/recuperar',
    '/Auth/cambiar-password'
  ];

  const esPublica = rutasPublicas.some(
    ruta => req.url.includes(ruta)
  );

  // TOKEN

  const token =
    sessionStorage.getItem('token') ||
    localStorage.getItem('token');

  // AUTORIZACIÓN

  let headers: Record<string, string> = {};

  if (token && !esPublica) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  // IDEMPOTENCIA

  const esMetodoIdempotente =
    req.method === 'POST' ||
    req.method === 'PUT';

  const esAuth =
    req.url.includes('/Auth/login') ||
    req.url.includes('/Auth/recuperar') ||
    req.url.includes('/Auth/cambiar-password');

  if (
    esMetodoIdempotente &&
    !esAuth &&
    !req.headers.has('Idempotency-Key')
  ) {
    headers['Idempotency-Key'] = crypto.randomUUID();
  }

  // CLONAR REQUEST

  const authReq = Object.keys(headers).length > 0
    ? req.clone({
      setHeaders: headers
    })
    : req;

  // ENVIAR REQUEST

  return next(authReq).pipe(

    catchError((error: HttpErrorResponse) => {

      if (
        error.status === 401 &&
        !esPublica &&
        token
      ) {

        // Solo limpiar sesión.
        // Sin reload.
        sessionStorage.clear();

        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('rol');
      }

      return throwError(() => error);
    })

  );
};