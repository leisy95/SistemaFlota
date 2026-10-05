import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerTrasladoNoFormalizado } from './ver-traslado-no-formalizado';

describe('VerTrasladoNoFormalizado', () => {
  let component: VerTrasladoNoFormalizado;
  let fixture: ComponentFixture<VerTrasladoNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerTrasladoNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerTrasladoNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
