import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardMateriales } from './dashboard-materiales';

describe('DashboardMateriales', () => {
  let component: DashboardMateriales;
  let fixture: ComponentFixture<DashboardMateriales>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardMateriales]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardMateriales);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
