import { Routes } from '@angular/router';
import { ShellLayout } from './core/layout/shell-layout/shell-layout';
import { MinimalLayout } from './core/layout/minimal-layout/minimal-layout';

export const routes: Routes = [
  // Public routes (minimal layout — no nav bar)
  {
    path: '',
    component: MinimalLayout,
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login').then(m => m.Login) },
      { path: 'register', loadComponent: () => import('./features/auth/register').then(m => m.Register) },
    ],
  },
  // Authenticated routes (shell layout — with nav bar)
  {
    path: '',
    component: ShellLayout,
    children: [
      { path: '', loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.dashboardRoutes) },
      { path: 'watch-spaces', loadChildren: () => import('./features/watch-spaces/watch-spaces.routes').then(m => m.watchSpacesRoutes) },
      { path: 'settings', loadChildren: () => import('./features/settings/settings.routes').then(m => m.settingsRoutes) },
      { path: 'showcase', loadChildren: () => import('./features/showcase/showcase.routes').then(m => m.showcaseRoutes) },
    ],
  },
];
