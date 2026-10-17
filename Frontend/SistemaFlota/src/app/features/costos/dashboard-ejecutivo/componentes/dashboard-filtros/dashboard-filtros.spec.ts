import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardFiltros } from './dashboard-filtros';

describe('DashboardFiltros', () => {
  let component: DashboardFiltros;
  let fixture: ComponentFixture<DashboardFiltros>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardFiltros]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardFiltros);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
