import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistorialCorteInventario } from './historial-corte-inventario';

describe('HistorialCorteInventario', () => {
  let component: HistorialCorteInventario;
  let fixture: ComponentFixture<HistorialCorteInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialCorteInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistorialCorteInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
