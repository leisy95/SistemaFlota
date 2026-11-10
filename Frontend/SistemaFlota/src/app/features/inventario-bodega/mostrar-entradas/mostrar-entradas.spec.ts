import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MostrarEntradas } from './mostrar-entradas';

describe('MostrarEntradas', () => {
  let component: MostrarEntradas;
  let fixture: ComponentFixture<MostrarEntradas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MostrarEntradas]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MostrarEntradas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
