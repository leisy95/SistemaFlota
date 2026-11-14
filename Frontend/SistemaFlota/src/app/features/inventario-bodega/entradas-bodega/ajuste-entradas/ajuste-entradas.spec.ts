import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AjusteEntradas } from './ajuste-entradas';

describe('AjusteEntradas', () => {
  let component: AjusteEntradas;
  let fixture: ComponentFixture<AjusteEntradas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AjusteEntradas]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AjusteEntradas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
