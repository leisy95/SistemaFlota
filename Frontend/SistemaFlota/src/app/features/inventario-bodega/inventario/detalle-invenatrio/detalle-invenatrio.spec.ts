import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleInvenatrio } from './detalle-invenatrio';

describe('DetalleInvenatrio', () => {
  let component: DetalleInvenatrio;
  let fixture: ComponentFixture<DetalleInvenatrio>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleInvenatrio]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleInvenatrio);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
