import { Routes } from '@angular/router';
import { Login } from './login';
import { Register } from './register';
import { ForgotPassword } from './forgot-password/forgot-password';
import { ResetPassword } from './reset-password/reset-password';

export const authRoutes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  // Note: these are also registered directly in app.routes.ts under MinimalLayout
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'reset-password', component: ResetPassword },
];
