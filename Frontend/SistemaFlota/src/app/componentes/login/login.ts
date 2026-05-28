import {
  Component,
  Output,
  EventEmitter
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})

export class LoginComponent {

  @Output()
  loginSuccess = new EventEmitter<any>();

  username = '';
  password = '';

  constructor(private auth: AuthService) {}

  iniciarSesion() {
    this.auth.login(this.username, this.password)
    .subscribe({
      next: (res: any) => {
        console.log('LOGIN OK ✅', res);

        localStorage.setItem('token', res.token);

        localStorage.setItem('user', JSON.stringify({
          username: res.username,
          rol:      res.rol,
          email:    res.email,
          permisos: res.permisos ?? []
        }));

        this.loginSuccess.emit(res);
      },
      error: (err) => {
        console.error('ERROR LOGIN ❌', err);
        alert('Usuario o contraseña incorrectos');
      }
    });
  }

}