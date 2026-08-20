import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerificarOrdenTraslado } from './verificar-orden-traslado';

describe('VerificarOrdenTraslado', () => {
  let component: VerificarOrdenTraslado;
  let fixture: ComponentFixture<VerificarOrdenTraslado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerificarOrdenTraslado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerificarOrdenTraslado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
