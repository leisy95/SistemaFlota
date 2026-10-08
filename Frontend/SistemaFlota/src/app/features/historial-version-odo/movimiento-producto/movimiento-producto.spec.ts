import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MovimientoProducto } from './movimiento-producto';

describe('MovimientoProducto', () => {
  let component: MovimientoProducto;
  let fixture: ComponentFixture<MovimientoProducto>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MovimientoProducto]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MovimientoProducto);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
