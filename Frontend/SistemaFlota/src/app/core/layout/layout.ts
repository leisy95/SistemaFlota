import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from './sidebar/sidebar';
import { Topbar } from './topbar/topbar';
import { pageSlideAnimation } from '../animations/pageSlideAnimation';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, Sidebar, Topbar],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
  animations: [pageSlideAnimation]
})
export class Layout {
  sidebarAbierto = false;
  sidebarColapsado = false;

  prepareRoute(outlet: RouterOutlet) {
    const animation = outlet?.activatedRouteData?.['animation'];

    if (animation === 'inicio') {
      return null;
    }

    return animation;
  }

  toggleMenu(): void {
    if (window.innerWidth <= 768) {
      this.sidebarAbierto = !this.sidebarAbierto;
      return;
    }
    this.sidebarColapsado = !this.sidebarColapsado;
  }

  toggleColapsoSidebar(): void { if (window.innerWidth > 768) { this.sidebarColapsado = !this.sidebarColapsado; } }

  cerrarSidebar(): void {
    this.sidebarAbierto = false;
  }
}