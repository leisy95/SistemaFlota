import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardIndicadores } from './dashboard-indicadores';

describe('DashboardIndicadores', () => {
  let component: DashboardIndicadores;
  let fixture: ComponentFixture<DashboardIndicadores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardIndicadores]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardIndicadores);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
