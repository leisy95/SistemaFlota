import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerificarOrdenTrasladoNoFormalizado } from './verificar-orden-traslado-no-formalizado';

describe('VerificarOrdenTrasladoNoFormalizado', () => {
  let component: VerificarOrdenTrasladoNoFormalizado;
  let fixture: ComponentFixture<VerificarOrdenTrasladoNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerificarOrdenTrasladoNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerificarOrdenTrasladoNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
