import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface DialogInputData {
  titulo: string;
  mensaje?: string;
  label: string;
  placeholder?: string;
  textoConfirmar?: string;
  textoCancelar?: string;
}

@Component({
  selector: 'app-dialog-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dialog-input.html',
  styleUrl: './dialog-input.scss',
})
export class DialogInput {
  valor = '';

  constructor(
    private dialogRef: MatDialogRef<DialogInput>,
    @Inject(MAT_DIALOG_DATA) public data: DialogInputData
  ) { }

  cancelar(): void {
    this.dialogRef.close(null);
  }

  confirmar(): void {
    if (!this.valor.trim()) return;
    this.dialogRef.close(this.valor.trim());
  }
}