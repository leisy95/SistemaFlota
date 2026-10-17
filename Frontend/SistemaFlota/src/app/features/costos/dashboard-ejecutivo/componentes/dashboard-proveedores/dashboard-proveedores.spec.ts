import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardProveedores } from './dashboard-proveedores';

describe('DashboardProveedores', () => {
  let component: DashboardProveedores;
  let fixture: ComponentFixture<DashboardProveedores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardProveedores]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardProveedores);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
