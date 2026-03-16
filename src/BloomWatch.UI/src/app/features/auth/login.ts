import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [RouterLink],
  template: `
    <h1>Login</h1>
    <p>Login form coming soon.</p>
    <p><a routerLink="/register">Create an account</a></p>
  `,
})
export class Login {}
