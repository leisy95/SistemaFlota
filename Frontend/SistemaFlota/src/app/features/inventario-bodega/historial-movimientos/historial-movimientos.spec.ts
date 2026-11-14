import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistorialMovimientos } from './historial-movimientos';

describe('HistorialMovimientos', () => {
  let component: HistorialMovimientos;
  let fixture: ComponentFixture<HistorialMovimientos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialMovimientos]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistorialMovimientos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
