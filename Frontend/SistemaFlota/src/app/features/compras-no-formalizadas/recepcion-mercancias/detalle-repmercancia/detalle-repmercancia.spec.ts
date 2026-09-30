import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalleRepmercancia } from './detalle-repmercancia';

describe('DetalleRepmercancia', () => {
  let component: DetalleRepmercancia;
  let fixture: ComponentFixture<DetalleRepmercancia>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalleRepmercancia]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetalleRepmercancia);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
