import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccionesOrdenCompraNoFormalizada } from './acciones-orden-compra-no-formalizada';

describe('AccionesOrdenCompraNoFormalizada', () => {
  let component: AccionesOrdenCompraNoFormalizada;
  let fixture: ComponentFixture<AccionesOrdenCompraNoFormalizada>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccionesOrdenCompraNoFormalizada]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccionesOrdenCompraNoFormalizada);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
