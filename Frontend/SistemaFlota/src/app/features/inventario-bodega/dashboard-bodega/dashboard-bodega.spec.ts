import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardBodega } from './dashboard-bodega';

describe('DashboardBodega', () => {
  let component: DashboardBodega;
  let fixture: ComponentFixture<DashboardBodega>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardBodega]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardBodega);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
