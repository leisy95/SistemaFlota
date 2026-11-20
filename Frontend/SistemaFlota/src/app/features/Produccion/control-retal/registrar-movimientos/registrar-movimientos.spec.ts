import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegistrarMovimientos } from './registrar-movimientos';

describe('RegistrarMovimientos', () => {
  let component: RegistrarMovimientos;
  let fixture: ComponentFixture<RegistrarMovimientos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegistrarMovimientos]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegistrarMovimientos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
