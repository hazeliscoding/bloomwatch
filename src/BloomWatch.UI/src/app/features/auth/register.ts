import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [RouterLink],
  template: `
    <h1>Register</h1>
    <p>Registration form coming soon.</p>
    <p><a routerLink="/login">Already have an account?</a></p>
  `,
})
export class Register {}
