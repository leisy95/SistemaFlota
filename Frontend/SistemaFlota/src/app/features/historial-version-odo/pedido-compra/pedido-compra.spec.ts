import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PedidoCompra } from './pedido-compra';

describe('PedidoCompra', () => {
  let component: PedidoCompra;
  let fixture: ComponentFixture<PedidoCompra>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PedidoCompra]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PedidoCompra);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
