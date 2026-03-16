import { Routes } from '@angular/router';
import { Login } from './login';
import { Register } from './register';

export const authRoutes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
];
