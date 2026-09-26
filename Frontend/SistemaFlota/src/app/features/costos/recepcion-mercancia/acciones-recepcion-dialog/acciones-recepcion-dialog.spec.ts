import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccionesRecepcionDialog } from './acciones-recepcion-dialog';

describe('AccionesRecepcionDialog', () => {
  let component: AccionesRecepcionDialog;
  let fixture: ComponentFixture<AccionesRecepcionDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccionesRecepcionDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccionesRecepcionDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
