import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProveedoresMateriales } from './proveedores-materiales';

describe('ProveedoresMateriales', () => {
  let component: ProveedoresMateriales;
  let fixture: ComponentFixture<ProveedoresMateriales>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProveedoresMateriales]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProveedoresMateriales);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
