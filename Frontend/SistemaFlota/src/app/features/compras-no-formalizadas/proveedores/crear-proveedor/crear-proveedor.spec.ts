import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearProveedor } from './crear-proveedor';

describe('CrearProveedor', () => {
  let component: CrearProveedor;
  let fixture: ComponentFixture<CrearProveedor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearProveedor]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearProveedor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
