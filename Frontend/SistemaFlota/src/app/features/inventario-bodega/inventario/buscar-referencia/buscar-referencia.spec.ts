import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuscarReferencia } from './buscar-referencia';

describe('BuscarReferencia', () => {
  let component: BuscarReferencia;
  let fixture: ComponentFixture<BuscarReferencia>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BuscarReferencia]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BuscarReferencia);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
