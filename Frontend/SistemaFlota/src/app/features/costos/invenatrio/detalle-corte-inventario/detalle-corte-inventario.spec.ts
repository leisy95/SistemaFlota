import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleCorteInventario } from './detalle-corte-inventario';

describe('DetalleCorteInventario', () => {
  let component: DetalleCorteInventario;
  let fixture: ComponentFixture<DetalleCorteInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleCorteInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleCorteInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
