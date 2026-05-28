import { Routes }
from '@angular/router';

import { authGuard }
from './auth.guard';

export const routes: Routes = [

  // LOGIN

  {
    path: 'login',

    loadComponent: () =>

      import('./componentes/login/login')
      .then(
        m => m.LoginComponent
      )
  },

  // CHECKLIST

  {
    path: 'checklist-editor',

    canActivate: [
      authGuard
    ],

    loadComponent: () =>

      import(
        './componentes/checklist-editor/checklist-editor'
      ).then(
        m => m.ChecklistEditorComponent
      )
  },

  // REDIRECT

  {
    path: '**',

    redirectTo: 'login'
  }

];