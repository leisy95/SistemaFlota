import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistorialInventarioNoFormalizado } from './historial-inventario-no-formalizado';

describe('HistorialInventarioNoFormalizado', () => {
  let component: HistorialInventarioNoFormalizado;
  let fixture: ComponentFixture<HistorialInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistorialInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
