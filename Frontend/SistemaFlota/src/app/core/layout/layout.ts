import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from './sidebar/sidebar';
import { Topbar } from './topbar/topbar';
import { trigger, transition, style, animate, query, group } from '@angular/animations';
import { pageSlideAnimation } from '../animations/pageSlideAnimation';


@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    Sidebar,
    Topbar,
  ],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',

  animations: [
    pageSlideAnimation
  ]
})

export class Layout {

  sidebarAbierto = false;

  prepareRoute(outlet: RouterOutlet) {

    const animation = outlet?.activatedRouteData?.['animation'];

    if (animation === 'inicio') {
      return null;
    }

    return animation;

  }

  toggleSidebar() {
    this.sidebarAbierto = !this.sidebarAbierto;
  }

  cerrarSidebar() {
    this.sidebarAbierto = false;
  }
}
