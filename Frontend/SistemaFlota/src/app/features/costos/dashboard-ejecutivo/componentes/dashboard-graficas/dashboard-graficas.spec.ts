import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardGraficas } from './dashboard-graficas';

describe('DashboardGraficas', () => {
  let component: DashboardGraficas;
  let fixture: ComponentFixture<DashboardGraficas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardGraficas]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardGraficas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
