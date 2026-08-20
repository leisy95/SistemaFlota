import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AjusteInventario } from './ajuste-inventario';

describe('AjusteInventario', () => {
  let component: AjusteInventario;
  let fixture: ComponentFixture<AjusteInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AjusteInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AjusteInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
