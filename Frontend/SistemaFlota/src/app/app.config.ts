import {
  ApplicationConfig,
  provideZoneChangeDetection,
  isDevMode
} from '@angular/core';
import { provideRouter }        from '@angular/router';
import { provideHttpClient,
         withInterceptors }     from '@angular/common/http';
import { provideServiceWorker } from '@angular/service-worker';
import { authInterceptor }      from './interceptors/auth.interceptor';
import { routes }               from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideServiceWorker('ngsw-worker.js', {
      enabled: false,
      registrationStrategy: 'registerWhenStable:30000'
    })
  ]
};