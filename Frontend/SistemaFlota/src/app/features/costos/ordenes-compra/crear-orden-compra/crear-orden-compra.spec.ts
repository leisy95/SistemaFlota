import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearOrdenCompra } from './crear-orden-compra';

describe('CrearOrdenCompra', () => {
  let component: CrearOrdenCompra;
  let fixture: ComponentFixture<CrearOrdenCompra>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearOrdenCompra]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearOrdenCompra);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
