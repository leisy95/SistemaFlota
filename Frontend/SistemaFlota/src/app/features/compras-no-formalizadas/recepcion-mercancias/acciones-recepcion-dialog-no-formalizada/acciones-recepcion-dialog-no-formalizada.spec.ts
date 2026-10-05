import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccionesRecepcionDialogNoFormalizada } from './acciones-recepcion-dialog-no-formalizada';

describe('AccionesRecepcionDialogNoFormalizada', () => {
  let component: AccionesRecepcionDialogNoFormalizada;
  let fixture: ComponentFixture<AccionesRecepcionDialogNoFormalizada>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccionesRecepcionDialogNoFormalizada]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccionesRecepcionDialogNoFormalizada);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
