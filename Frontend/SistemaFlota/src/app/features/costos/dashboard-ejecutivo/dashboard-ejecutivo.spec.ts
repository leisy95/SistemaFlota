import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardEjecutivo } from './dashboard-ejecutivo';

describe('DashboardEjecutivo', () => {
  let component: DashboardEjecutivo;
  let fixture: ComponentFixture<DashboardEjecutivo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardEjecutivo]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardEjecutivo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
