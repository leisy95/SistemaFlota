import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistorialInventario } from './historial-inventario';

describe('HistorialInventario', () => {
  let component: HistorialInventario;
  let fixture: ComponentFixture<HistorialInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistorialInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
