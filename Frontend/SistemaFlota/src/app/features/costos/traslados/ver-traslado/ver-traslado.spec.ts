import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerTraslado } from './ver-traslado';

describe('VerTraslado', () => {
  let component: VerTraslado;
  let fixture: ComponentFixture<VerTraslado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerTraslado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerTraslado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
