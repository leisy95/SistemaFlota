import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CorteInventario } from './corte-inventario';

describe('CorteInventario', () => {
  let component: CorteInventario;
  let fixture: ComponentFixture<CorteInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CorteInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CorteInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
