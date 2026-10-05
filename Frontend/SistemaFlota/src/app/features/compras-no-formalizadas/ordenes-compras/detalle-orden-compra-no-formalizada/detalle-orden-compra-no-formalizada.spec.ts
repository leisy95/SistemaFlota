import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleOrdenCompraNoFormalizada } from './detalle-orden-compra-no-formalizada';

describe('DetalleOrdenCompraNoFormalizada', () => {
  let component: DetalleOrdenCompraNoFormalizada;
  let fixture: ComponentFixture<DetalleOrdenCompraNoFormalizada>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleOrdenCompraNoFormalizada]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleOrdenCompraNoFormalizada);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
