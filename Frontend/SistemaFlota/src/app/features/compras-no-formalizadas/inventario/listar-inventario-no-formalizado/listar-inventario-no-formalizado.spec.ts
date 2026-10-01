import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarInventarioNoFormalizado } from './listar-inventario-no-formalizado';

describe('ListarInventarioNoFormalizado', () => {
  let component: ListarInventarioNoFormalizado;
  let fixture: ComponentFixture<ListarInventarioNoFormalizado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarInventarioNoFormalizado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarInventarioNoFormalizado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
