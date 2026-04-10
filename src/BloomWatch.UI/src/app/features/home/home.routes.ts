import { Routes } from '@angular/router';

export const homeRoutes: Routes = [
  { path: '', title: 'Home', loadComponent: () => import('./home').then(m => m.Home) },
];
