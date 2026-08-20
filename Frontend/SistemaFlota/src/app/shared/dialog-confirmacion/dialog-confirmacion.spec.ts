import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogConfirmacion } from './dialog-confirmacion';

describe('DialogConfirmacion', () => {
  let component: DialogConfirmacion;
  let fixture: ComponentFixture<DialogConfirmacion>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogConfirmacion]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DialogConfirmacion);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
