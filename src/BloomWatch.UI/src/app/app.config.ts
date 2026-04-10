import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { TitleStrategy } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/http/auth.interceptor';
import { BloomTitleStrategy } from './core/title/bloom-title-strategy';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: TitleStrategy, useClass: BloomTitleStrategy },
  ]
};
