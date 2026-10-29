import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleMovimiento } from './detalle-movimiento';

describe('DetalleMovimiento', () => {
  let component: DetalleMovimiento;
  let fixture: ComponentFixture<DetalleMovimiento>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleMovimiento]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleMovimiento);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
