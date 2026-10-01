import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CorteInventarioNoFormalizado } from './corte-inventario-no-formalizado';

describe('CorteInventarioNoFormalizado', () => {
  let component: CorteInventarioNoFormalizado;
  let fixture: ComponentFixture<CorteInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CorteInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CorteInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
