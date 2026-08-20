import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccionesOrdenCompra } from './acciones-orden-compra';

describe('AccionesOrdenCompra', () => {
  let component: AccionesOrdenCompra;
  let fixture: ComponentFixture<AccionesOrdenCompra>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccionesOrdenCompra]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccionesOrdenCompra);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
