import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearTrasladoNoFormalizado } from './crear-traslado-no-formalizado';

describe('CrearTrasladoNoFormalizado', () => {
  let component: CrearTrasladoNoFormalizado;
  let fixture: ComponentFixture<CrearTrasladoNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearTrasladoNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearTrasladoNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
