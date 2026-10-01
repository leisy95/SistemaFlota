import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AjusteInventarioNoFormalizado } from './ajuste-inventario-no-formalizado';

describe('AjusteInventarioNoFormalizado', () => {
  let component: AjusteInventarioNoFormalizado;
  let fixture: ComponentFixture<AjusteInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AjusteInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AjusteInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
