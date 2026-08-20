import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleOrdenCompra } from './detalle-orden-compra';

describe('DetalleOrdenCompra', () => {
  let component: DetalleOrdenCompra;
  let fixture: ComponentFixture<DetalleOrdenCompra>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleOrdenCompra]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleOrdenCompra);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
