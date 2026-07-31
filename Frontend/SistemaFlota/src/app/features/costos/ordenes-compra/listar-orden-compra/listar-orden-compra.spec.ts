import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarOrdenCompra } from './listar-orden-compra';

describe('ListarOrdenCompra', () => {
  let component: ListarOrdenCompra;
  let fixture: ComponentFixture<ListarOrdenCompra>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarOrdenCompra]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarOrdenCompra);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
