import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistorialCorteInventarioNoFormalizado } from './historial-corte-inventario-no-formalizado';

describe('HistorialCorteInventarioNoFormalizado', () => {
  let component: HistorialCorteInventarioNoFormalizado;
  let fixture: ComponentFixture<HistorialCorteInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialCorteInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistorialCorteInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
