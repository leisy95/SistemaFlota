
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

  // IDENTIFICAR LAS DOS SECCIONES CON SESIÓN EXTENDIDA
  const rutaActual = window.location.pathname;

  const usaSesionExtendida =
    rutaActual.startsWith('/inventario-bodega/registro-salidas') ||
    rutaActual.startsWith('/inventario-bodega/registrar-entrada');

  // SELECCIONAR TOKEN
  const tokenNormal =
    sessionStorage.getItem('token') ||
    localStorage.getItem('token');

  const tokenBodega =
    sessionStorage.getItem('tokenBodega');

  const usaTokenBodega =
    usaSesionExtendida && !!tokenBodega;

  const token = usaTokenBodega
    ? tokenBodega
    : tokenNormal;

  // AUTORIZACIÓN
  let headers: Record<string, string> = {};

  if (token && !esPublica) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  // IDEMPOTENCIA
  const esMetodoIdempotente =
    req.method === 'POST' ||
    req.method === 'PUT';

  const esAuth = rutasPublicas.some(
    ruta => req.url.includes(ruta)
  );

  if (
    esMetodoIdempotente &&
    !esAuth &&
    !req.headers.has('Idempotency-Key')
  ) {
    headers['Idempotency-Key'] = crypto.randomUUID();
  }

  // CLONAR REQUEST
  const authReq = Object.keys(headers).length > 0
    ? req.clone({ setHeaders: headers })
    : req;

  // ENVIAR REQUEST
  return next(authReq).pipe(

    catchError((error: HttpErrorResponse) => {

      if (error.status === 401 && !esPublica && token) {

        if (usaTokenBodega) {

          // conservar la sesión normal.
          sessionStorage.removeItem('tokenBodega');

        } else {

          sessionStorage.clear();

          localStorage.removeItem('token');
          localStorage.removeItem('tokenBodega');
          localStorage.removeItem('user');
          localStorage.removeItem('rol');
        }
      }

      return throwError(() => error);
    })

  );
};
