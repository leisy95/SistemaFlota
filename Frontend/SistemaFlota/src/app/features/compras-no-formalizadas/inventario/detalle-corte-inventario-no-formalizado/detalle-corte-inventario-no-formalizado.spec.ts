import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleCorteInventarioNoFormalizado } from './detalle-corte-inventario-no-formalizado';

describe('DetalleCorteInventarioNoFormalizado', () => {
  let component: DetalleCorteInventarioNoFormalizado;
  let fixture: ComponentFixture<DetalleCorteInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleCorteInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleCorteInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
