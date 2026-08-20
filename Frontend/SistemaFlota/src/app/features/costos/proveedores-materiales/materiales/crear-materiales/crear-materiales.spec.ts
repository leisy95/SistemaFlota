import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearMateriales } from './crear-materiales';

describe('CrearMateriales', () => {
  let component: CrearMateriales;
  let fixture: ComponentFixture<CrearMateriales>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearMateriales]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearMateriales);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
