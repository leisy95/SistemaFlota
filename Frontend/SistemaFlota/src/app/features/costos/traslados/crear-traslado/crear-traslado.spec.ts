import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearTraslado } from './crear-traslado';

describe('CrearTraslado', () => {
  let component: CrearTraslado;
  let fixture: ComponentFixture<CrearTraslado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearTraslado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearTraslado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
