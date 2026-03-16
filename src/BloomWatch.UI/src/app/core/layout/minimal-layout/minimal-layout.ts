import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-minimal-layout',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class MinimalLayout {}
